using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Level
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {
        private LevelManager myObject;
        private const int ElementHeight = 50;
        private bool _foldListObject;
        private List<bool> _foldListObjects = new List<bool>();
        private ObjectPhysics[] allObjectPhysics;

        // Object Preview
        Editor[] gameObjectsEditor;

        ReorderableList[] list;
        
        private List<List<ObjectPhysics>> LevelObjectPhysics;
        private int _maxLevel = 0;
        private int _currentDrawedLevel;
        private float scalePlayerSupposition = 1;


        int GetNumberOfObjectInLevel(int level)
        {
            int count = 0;
            for (int i = 0; i < allObjectPhysics.Length; i++)
            {
                if (allObjectPhysics[i].SleepUntilLevel == level)
                    count++;
            }

            return count;
        }
        ObjectPhysics[] GetObjectsInLevel(int level)
        {
            ObjectPhysics[] objectPhysics = new ObjectPhysics[GetNumberOfObjectInLevel(level)];
            int index = 0;
            for (int i = 0; i < allObjectPhysics.Length; i++)
            {
                if (allObjectPhysics[i].SleepUntilLevel == level)
                {
                    objectPhysics[index] = allObjectPhysics[i];
                    index++;
                }
                   
            }

            return objectPhysics;
        }
        
        
        private void OnEnable()
        {
            allObjectPhysics = GameObject.FindObjectsOfType<ObjectPhysics>();
            
            // ReorderableList Setup
            int lenght = GetMaxLevel();
            list = new ReorderableList[lenght];
            
            for (int i = 0; i < lenght; i++)
            {
                var objsInLevel = GetObjectsInLevel(i);
                if (objsInLevel.Length == 0) continue;
                 list[i] = new ReorderableList(objsInLevel, typeof(ObjectPhysics),true,true,true,true);
                 list[i].drawElementCallback = DrawElementItem; // Delegate to draw the elements on the list
                 list[i].drawHeaderCallback = DrawHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
                 list[i].elementHeight = ElementHeight;
            }
            
            // Init Editor[] for object icon preview
            if (gameObjectsEditor == null || gameObjectsEditor.Length != allObjectPhysics.Length)
            {
                gameObjectsEditor = new Editor[allObjectPhysics.Length];
            }

        }

        #region ReordonableListCallback

        void DrawElementItem(Rect rect, int index, bool isActive, bool isFocused)
        {
           
            // Get serialize object and properties
            var serializeObjectPhy = new SerializedObject( GetObjectsInLevel(_currentDrawedLevel)[index]);
            var propertyForce = serializeObjectPhy.FindProperty("forceRequired");
            var propertyMultiplier = serializeObjectPhy.FindProperty("scaleMultiplier");
            
            // Draw object preview
            if (allObjectPhysics[index].gameObject != null)
            {
                // Check if the object have his editor for the preview
                if (gameObjectsEditor[index] == null)
                    gameObjectsEditor[index] = CreateEditor(allObjectPhysics[index].gameObject);
                
                Rect previewRect = rect;
                previewRect.width = 32;
                gameObjectsEditor[index].OnInteractivePreviewGUI(previewRect, new GUIStyle());
            }
            
            // GameObject name field
            Rect textFieldRect = new Rect(rect.x+32 , rect.y, 150, EditorGUIUtility.singleLineHeight);
            allObjectPhysics[index].gameObject.name = EditorGUI.TextField(textFieldRect, allObjectPhysics[index].gameObject.name);
            
            // ForceRequired Slider
            Rect forceRect = new Rect(rect.x+32, rect.y+16, rect.width / 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField( forceRect, "Force required");
           
            forceRect.x += forceRect.width / 2;
            propertyForce.floatValue = EditorGUI.Slider(forceRect,propertyForce.floatValue,1,2);
            serializeObjectPhy.ApplyModifiedProperties();
            
            // MultiplierGain Slider
            Rect MultiplierRect = new Rect(rect.x + 32, forceRect.y + 16, rect.width / 2,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField( MultiplierRect, "Scale&Force Gain");
           
            MultiplierRect.x += MultiplierRect.width / 2;
            propertyMultiplier.floatValue = EditorGUI.Slider(MultiplierRect,propertyMultiplier.floatValue,0,2);
            serializeObjectPhy.ApplyModifiedProperties();
            
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            float scale = 1;
            ObjectPhysics[] obj = GetObjectsInLevel(_currentDrawedLevel);
            for (int i = 0; i < obj.Length; i++)
            {
                scalePlayerSupposition += obj[i].ScaleMultiplier;
            }
            string name = $"Objects available in level {_currentDrawedLevel} / TOTAL Scale player at the end : {scalePlayerSupposition} ";
            EditorGUI.LabelField(rect, name);
        }
        

        #endregion

        
        
        public override void OnInspectorGUI()
        {
            myObject = (LevelManager) target;
            myObject.transform.position = Vector3.zero; 
            myObject.transform.rotation = Quaternion.identity;
            GetMaxLevel();

            if (GetMaxLevel() != list.Length)
            {
                int lenght = GetMaxLevel();
                list = new ReorderableList[lenght];
            
                for (int i = 0; i < lenght; i++)
                {
                    var objsInLevel = GetObjectsInLevel(i);
                    if (objsInLevel.Length == 0) continue;
                    list[i] = new ReorderableList(objsInLevel, typeof(ObjectPhysics),true,true,true,true);
                    list[i].drawElementCallback = DrawElementItem; // Delegate to draw the elements on the list
                    list[i].drawHeaderCallback = DrawHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
                    list[i].elementHeight = ElementHeight;
                }
            }
            base.OnInspectorGUI();
            
            var soObject = new SerializedObject(myObject);
            var propertyFloorCollision =  soObject.FindProperty("floorCollision");
            
            for (int i = 0; i < propertyFloorCollision.arraySize; i++)
            {
                propertyFloorCollision.GetArrayElementAtIndex(i).objectReferenceValue.name = "Floor Collision Level " + i;
            }

            if (GUILayout.Button("Create Waypoint Align to current view"))
            {
                GameObject myWaypoint = Instantiate(new GameObject(), myObject.transform);
                var transformCamScene = SceneView.lastActiveSceneView.camera.transform;
                if (transformCamScene != null)
                {
                    myWaypoint.transform.position = transformCamScene.position;
                    myWaypoint.transform.rotation = transformCamScene.rotation;
                    myObject.AddWaypoint(myWaypoint.transform);
                }
                else
                {
                    Debug.LogError("Failed to create waypoint");
                }
            }

            if (GUILayout.Button("Next Waypoint"))
            {
                myObject.RemoveAllObjectPhysical();
            }

            if (Application.isPlaying) return;
            
            if (GUILayout.Button("Generate Cinemachine Travelling"))
            {
                CinemachineSmoothPath cinemachineSmoothPath;
                cinemachineSmoothPath = myObject.TryGetComponent<CinemachineSmoothPath>(out CinemachineSmoothPath smoothPath) 
                                        ? smoothPath : myObject.AddComponent<CinemachineSmoothPath>();

                cinemachineSmoothPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[myObject.waypoints.Count];
                for (int i = 0; i < myObject.waypoints.Count; i++)
                {
                    CinemachineSmoothPath.Waypoint waypoint = new CinemachineSmoothPath.Waypoint();
                    waypoint.position = myObject.waypoints[i].position;

                    cinemachineSmoothPath.m_Waypoints[i] = waypoint;
                }
            }

            _foldListObject = EditorGUILayout.Foldout(_foldListObject, "List of Object");
            if (_foldListObject)
            {
                _currentDrawedLevel = 0;
                scalePlayerSupposition = 1;
                int lenght = GetMaxLevel();
                for (int i = 0; i < lenght; i++)
                {
                    Debug.Log($"[LevelManagerEditor] Draw reorderable list [{i}]");
                    if (GetObjectsInLevel(i).Length == 0)
                    {
                        _currentDrawedLevel++;
                        continue;
                    }
                    list[i].DoLayoutList();
                    _currentDrawedLevel++;
                }
                
            }
            
            EditorGUILayout.HelpBox($"The final Player scale is : {scalePlayerSupposition}", MessageType.Info);
        }
        /// <summary>
        /// Get max level available for the scene
        /// </summary>
        int GetMaxLevel()
        {
            _maxLevel = 0;
            for (int i = 0; i < allObjectPhysics.Length; i++)
            {
                if (allObjectPhysics[i].SleepUntilLevel+1 >= _maxLevel)
                {
                    _maxLevel = allObjectPhysics[i].SleepUntilLevel+1;
                }
            }

            return _maxLevel;
        }
        private static bool IsEmpty(ObjectPhysics[] allObjectPhysics, int i)
        {
            bool isVoid = true;
            foreach (var VARIABLE in allObjectPhysics)
            {
                if (i == VARIABLE.SleepUntilLevel)
                {
                    isVoid = false;
                }
            }

            return isVoid;
        }
        
    }
}