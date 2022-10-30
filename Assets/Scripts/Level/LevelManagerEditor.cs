using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Level
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {
        private LevelManager myObject;
        private bool _foldListObject;
        private List<bool> _foldListObjects = new List<bool>();

        // Object Preview

        Editor[] gameObjectsEditor;

        public override void OnInspectorGUI()
        {
            myObject = (LevelManager) target;
            myObject.transform.position = Vector3.zero; 
            myObject.transform.rotation = Quaternion.identity;

            base.OnInspectorGUI();


            for (int i = 0; i < myObject.floorCollision.Count; i++)
            {
                myObject.floorCollision[i].name = "Floor Collision Level " + i;
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

            if (GUILayout.Button("Generate Cinemachine Travelling"))
            {
                CinemachineSmoothPath cinemachineSmoothPath;
                if (myObject.TryGetComponent<CinemachineSmoothPath>(out CinemachineSmoothPath smoothPath))
                {
                    cinemachineSmoothPath = smoothPath;
                }
                else
                {
                    cinemachineSmoothPath = myObject.AddComponent<CinemachineSmoothPath>();
                }

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
                ObjectPhysics[] allObjectPhysics = GameObject.FindObjectsOfType<ObjectPhysics>();
                if (gameObjectsEditor == null || gameObjectsEditor.Length != allObjectPhysics.Length)
                {
                    gameObjectsEditor = new Editor[allObjectPhysics.Length];
                }


                int maxLevel = myObject.MaxLevel;

                for (int i = 0; i < allObjectPhysics.Length; i++)
                {
                    if (allObjectPhysics[i]._settings.sleepUntilLevel > maxLevel)
                    {
                        maxLevel = allObjectPhysics[i]._settings.sleepUntilLevel;
                    }
                }

                if (_foldListObjects.Count != maxLevel)
                {
                    for (int i = 0; i < maxLevel; i++)
                    {
                        _foldListObjects.Add(new bool());
                    }
                }

                for (int i = 0; i <= maxLevel; i++)
                {
                    EditorGUILayout.Space(3);

                    _foldListObjects[i] = EditorGUILayout.Foldout(_foldListObjects[i], "LEVEL : " + i);
                    bool isVoid = true;
                    foreach (var VARIABLE in allObjectPhysics)
                    {
                        if (i == VARIABLE._settings.sleepUntilLevel)
                        {
                            isVoid = false;
                        }
                    }

                    if (isVoid)
                    {
                        EditorGUILayout.HelpBox("WARNING, WE NEED TO PUT OBJECT IN THE LEVEL : " + i,
                            MessageType.Warning);
                        _foldListObjects[i] = true;
                        continue;
                    }

                    if (_foldListObjects[i])
                    {
                        for (int j = 0; j < allObjectPhysics.Length; j++)
                        {
                            var objectPhysic = allObjectPhysics[j];

                            if (i == objectPhysic._settings.sleepUntilLevel)
                            {
                                isVoid = false;
                                using (new GUILayout.HorizontalScope(EditorStyles.whiteLabel,
                                           GUILayout.ExpandWidth(true)))
                                {
                                    CreatePreviewObject(objectPhysic, j);
                                    if (GUILayout.Button($"{objectPhysic.name}"))
                                    {
                                        Selection.activeTransform = objectPhysic.transform;
                                    }

                                    using (new GUILayout.HorizontalScope(EditorStyles.whiteLabel,
                                               GUILayout.ExpandWidth(true)))
                                    {
                                        EditorGUILayout.LabelField("Force Required :");
                                        objectPhysic._settings.ForceRequired =
                                            EditorGUILayout.FloatField(objectPhysic._settings.ForceRequired);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreatePreviewObject(ObjectPhysics objectPhysic, int j)
        {
            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = EditorGUIUtility.whiteTexture;

            if (objectPhysic.gameObject != null)
            {
                if (gameObjectsEditor[j] == null)
                    gameObjectsEditor[j] = Editor.CreateEditor(objectPhysic.gameObject);

                gameObjectsEditor[j].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(32, 32), bgColor);
            }
        }
    }
}