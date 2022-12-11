using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        private int _indexObjectPreview;

        ReorderableList[] list;
        
        private List<List<ObjectPhysics>> LevelObjectPhysics;
        private int _maxLevel = 0;
        private int _currentDrawedLevel;
        private float scalePlayerSupposition = 1;
        private SerializedObject soObject;
        
        private const string ContainerNameFloorCollision = "Floor Collision container";

        private bool _showIconPreview;

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
        ObjectPhysics[] GetObjectsInLevel(int level, bool OrderByMultiplier = false)
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

            if (OrderByMultiplier)
            {
                IEnumerable<ObjectPhysics> query = objectPhysics.OrderBy(x => x.ScaleMultiplier);
                objectPhysics = query.ToArray();
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
                 list[i] = new ReorderableList(objsInLevel, typeof(ObjectPhysics),true,true,false,false);
                 list[i].drawElementCallback = DrawElementItem; // Delegate to draw the elements on the list
                 list[i].drawHeaderCallback = DrawHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
                 list[i].elementHeight = ElementHeight;
                 list[i].onChangedCallback = reorderableList =>
                 {
                     list[i].Deselect(list[i].index);
                 };
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
            var objectPhysics = GetObjectsInLevel(_currentDrawedLevel)[index];
            // Get serialize object and properties
            var serializeObjectPhy = new SerializedObject( objectPhysics);
            var propertyForce = serializeObjectPhy.FindProperty("forceRequired");
            var propertyMultiplier = serializeObjectPhy.FindProperty("scaleMultiplier");
            if (_showIconPreview)
            {
                // Draw object preview
                if (objectPhysics.gameObject != null)
                {
                    // Check if the object have his editor for the preview
                    if (gameObjectsEditor[_indexObjectPreview] == null)
                        gameObjectsEditor[_indexObjectPreview] = CreateEditor(objectPhysics.gameObject);
                
                    Rect previewRect = rect;
                    previewRect.width = 32;
                    gameObjectsEditor[_indexObjectPreview].OnInteractivePreviewGUI(previewRect, new GUIStyle());
                }
            }
            
            // GameObject name field
            Rect textFieldRect = new Rect(rect.x+32 , rect.y, 150, EditorGUIUtility.singleLineHeight);
            objectPhysics.gameObject.name = EditorGUI.TextField(textFieldRect, objectPhysics.gameObject.name);
            
            // ForceRequired Slider
            Rect forceRect = new Rect(rect.x+32, rect.y+16, rect.width / 2, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField( forceRect, $"Force required : {propertyForce.floatValue}");
           
            // forceRect.x += forceRect.width / 2;
            // propertyForce.floatValue = EditorGUI.Slider(forceRect,propertyForce.floatValue,0,1);
            // serializeObjectPhy.ApplyModifiedProperties();
            // MultiplierGain Slider
            Rect MultiplierRect = new Rect(rect.x + 32, forceRect.y + 16, rect.width / 2,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField( MultiplierRect, "Tiny gain / Big Gain");
           
            MultiplierRect.x += MultiplierRect.width / 2;
            propertyMultiplier.floatValue = EditorGUI.Slider(MultiplierRect,propertyMultiplier.floatValue,0,1);
            MultiplierRect.x += MultiplierRect.width;
            EditorGUI.LabelField(MultiplierRect, GetAdditionalScale(index).ToString() );
            
            serializeObjectPhy.ApplyModifiedProperties();
            _indexObjectPreview++;
        }
        
        //Draws the header
        void DrawHeader(Rect rect)
        {
            rect.width -= 30;
            ObjectPhysics[] obj = GetObjectsInLevel(_currentDrawedLevel);
       
            for (int i = 0; i < obj.Length; i++)
            {
                scalePlayerSupposition += obj[i].ScaleMultiplier;
            }
            var labelName = $"Objects available in level {_currentDrawedLevel} / TOTAL Scale player at the end : {scalePlayerSupposition} ";
            EditorGUI.LabelField(rect, labelName);
            rect.x += 250;
            rect.width -=250;
            myObject.DataLevels[_currentDrawedLevel].shipScaleAtTheEnd = EditorGUI.FloatField(rect,     myObject.DataLevels[_currentDrawedLevel].shipScaleAtTheEnd);
        }
        

        #endregion

        float GetAdditionalScale(int index)
        {
            float startScale = 1;
            float endScale;
            if (_currentDrawedLevel >= 1)
            {
                startScale = myObject.DataLevels[_currentDrawedLevel-1].shipScaleAtTheEnd;
                endScale = myObject.DataLevels[_currentDrawedLevel].shipScaleAtTheEnd - startScale;
            }
            else
            {
                endScale = myObject.DataLevels[_currentDrawedLevel].shipScaleAtTheEnd;
            }
            
            ObjectPhysics[] rewards = GetObjectsInLevel(_currentDrawedLevel);
            float sommeTotal = 0;
            foreach(var reward in rewards) sommeTotal += reward.ScaleMultiplier;
            
            float addition = 0;
            addition +=  (rewards[index].ScaleMultiplier / sommeTotal)*endScale;
            

            return addition;
        }
        
        public override void OnInspectorGUI()
        {
            myObject = (LevelManager) target;
            myObject.transform.position = Vector3.zero; 
            myObject.transform.rotation = Quaternion.identity;
            soObject ??= new SerializedObject(myObject);
            base.OnInspectorGUI();
            if (myObject.DataLevels.Length == 0)
            {
                EditorGUILayout.HelpBox("Data levels are not initialized, please create a level.",MessageType.Info);
                Debug.LogError("Data levels are not initialized, please create a level.", target);
                return;
            }
            
            CreateContainerObjectPhysicsPerLevel();
            CreateContainerPlayableVolume();
            CreateContainerFloorCollision();

            string badSetupMessage = "";
            bool showit = false;
            for (int i = 0; i < myObject.DataLevels.Length; i++)
            {
                DataLevel dataLevel = myObject.DataLevels[i];
                string tempMessage = $"\n Level {i} :";
                bool addit = !(dataLevel.floorCollision && dataLevel.playableVolume && dataLevel.playerSpawnPoint);
                
                if(addit)
                {
                    showit = true;
                    if (!dataLevel.floorCollision)
                    {
                        tempMessage += "Floor collission missing // ";
                    }
                    if(!dataLevel.playableVolume)
                    {
                        tempMessage += "playableVolume missing // ";
                    }

                    if (!dataLevel.playerSpawnPoint)
                    {
                        tempMessage += "playerSpawnPoint missing // ";
                    }

                    badSetupMessage += tempMessage;
                }
            }
            if(showit)
                EditorGUILayout.HelpBox(badSetupMessage,MessageType.Warning);
            
            GetMaxLevel();

            if (GetMaxLevel() != list.Length)
            {
                int lenght = GetMaxLevel();
                list = new ReorderableList[lenght];
            
                for (int i = 0; i < lenght; i++)
                {
                    var objsInLevel = GetObjectsInLevel(i);
                    //if (objsInLevel.Length == 0) continue;
                    list[i] = new ReorderableList(objsInLevel, typeof(ObjectPhysics),true,true,true,true);
                    list[i].drawElementCallback = DrawElementItem; // Delegate to draw the elements on the list
                    list[i].drawHeaderCallback = DrawHeader; // Skip this line if you set displayHeader to 'false' in your ReorderableList constructor.
                    list[i].elementHeight = ElementHeight;
                }
            }
            
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Next Level"))
                {
                    myObject.Player.GetComponentInChildren<ScaleShip>().SetScaleFactor(myObject.DataLevels[myObject.CurrentLevel].shipScaleAtTheEnd);
                    myObject.RemoveAllObjectPhysical();
                }
            }

            if (Application.isPlaying) return;


            if (GUILayout.Button("Balance gain and force"))
            {
                // Verification to see if endscale from the previous level is less than the next one 
                if (myObject.DataLevels.Length > 1)
                {
                    float firstEndScale = myObject.DataLevels[0].shipScaleAtTheEnd;
                    for (int i = 1; i < myObject.DataLevels.Length; i++)
                    {
                        if (firstEndScale >= myObject.DataLevels[i].shipScaleAtTheEnd)
                        {
                            myObject.DataLevels[i].shipScaleAtTheEnd = firstEndScale + 0.1f;
                        }
                        firstEndScale = myObject.DataLevels[i].shipScaleAtTheEnd;
                    }
                }
                for (int i = 0; i < myObject.DataLevels.Length; i++)
                {
                    var objectPhysicsInLevel = GetObjectsInLevel(i);
                    // We retreived all objects with a scale gain > 0, to see if its balanced.
                    List<ObjectPhysics> importantObjects = new List<ObjectPhysics>();
                    foreach (var obj in objectPhysicsInLevel)
                    {
                        if(obj.ScaleMultiplier > 0 )
                            importantObjects.Add(obj);
                        
                    }
                    if (importantObjects.Count == 0)
                    {
                        var spofit = new SerializedObject(objectPhysicsInLevel[0]);
                        spofit.FindProperty("scaleMultiplier").floatValue = 1;
                        importantObjects.Add(objectPhysicsInLevel[0]);
                    }
                    var sdfsdfobjectPhysicsForThisLevel = importantObjects;
                    IEnumerable<ObjectPhysics> query = sdfsdfobjectPhysicsForThisLevel.OrderBy(x => x.ScaleMultiplier);
                    var orderedObjectPhysics = query.ToList();
                    float start = orderedObjectPhysics[0].ScaleMultiplier;
                    start = orderedObjectPhysics[0].ScaleMultiplier;
                    for(int j = 1 ; j < orderedObjectPhysics.Count-1; j++)
                    {
                        if(start >= orderedObjectPhysics[j].ScaleMultiplier)
                        {
                            start += orderedObjectPhysics[j].ScaleMultiplier;
                        }
                        else
                        { //0.7 - 1
                            // var spofit = new SerializedObject(orderedObjectPhysics[j-1]); 
                            // spofit.FindProperty("scaleMultiplier").floatValue += orderedObjectPhysics[j].ScaleMultiplier - start;
                            // start += orderedObjectPhysics[j].ScaleMultiplier;
                            // spofit.ApplyModifiedProperties();
                            // j--;
                            
                             var spofit = new SerializedObject(orderedObjectPhysics[j]); 
                             spofit.FindProperty("scaleMultiplier").floatValue =  start;
                             spofit.ApplyModifiedProperties();
                             j--;
                        }
                    }
                    // Verification to check if all objects are not a gain = 0
                    // Otherwise, we force a object to have 1 
                    if (orderedObjectPhysics[^1].ScaleMultiplier == 0)
                    {
                        var lastObject = new SerializedObject(orderedObjectPhysics[^1]);
                        lastObject.FindProperty("scaleMultiplier").floatValue = 1;
                        lastObject.ApplyModifiedProperties();
                    }
                    // Apply scaleMultiplier to the force required, Set forceRequired to 0 for object(s) with the less gain 
                    for (int j = 0; j < orderedObjectPhysics.Count; j++)
                    {
                        var f = orderedObjectPhysics[j];
                        var spofit = new SerializedObject(f);
                        if (j == 0)
                        {
                            spofit.FindProperty("forceRequired").floatValue = 0;
                        }
                        else
                        {
                            if (f.ScaleMultiplier == orderedObjectPhysics[0].ScaleMultiplier)
                            {
                                spofit.FindProperty("forceRequired").floatValue = 0;
                            }
                            else
                            {
                                spofit.FindProperty("forceRequired").floatValue = spofit.FindProperty("scaleMultiplier").floatValue;
                            }
                        }
                        spofit.ApplyModifiedProperties();
                    }
                   
                }
            }
            _foldListObject = EditorGUILayout.Foldout(_foldListObject, "List of Object");
            if (_foldListObject)
            {
                _showIconPreview = EditorGUILayout.Toggle("Show icon preview", _showIconPreview);
                _currentDrawedLevel = 0;
                scalePlayerSupposition = 1;
                int lenght = GetMaxLevel();
                _indexObjectPreview = 0;
                for (int i = 0; i < lenght; i++)
                {
                    //Debug.Log($"[LevelManagerEditor] Draw reorderable list [{i}]");
                     if (GetObjectsInLevel(i).Length == 0)
                     {
                         continue;
                     }
                     _currentDrawedLevel = i;
                    list[_currentDrawedLevel].DoLayoutList();
                }
                
            }
            
            EditorGUILayout.HelpBox($"The final Player scale is : {scalePlayerSupposition}", MessageType.Info);
        }

        #region Container

        private void CreateContainerObjectPhysicsPerLevel()
        {
            // Create container for ObjectPhysics per level
            var serializedProperty = soObject.FindProperty("dataLevels");
            int length = serializedProperty.arraySize;
            if (length == 0)
            {
                // No data level, so not necessary to create container
                return;
            }

            for (int i = 0; i < length; i++)
            {
                string containerName = "ObjectPhysics Level " + i;
                var container = GetContainer(containerName);
                if (GUILayout.Button($"Place Object of Level {i} in container"))
                {
                    foreach (var objPhy in GetObjectsInLevel(i))
                    {
                        objPhy.transform.parent = container.transform;
                    }
                }
                
            }
        }

        GameObject GetContainer(string name)
        {
            GameObject container = GameObject.Find(name);
            if (container == null)
            {
                container = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity, myObject.transform);
                container.name = name;
            }

            return container;
        }
        private void CreateContainerPlayableVolume()
        {
            int length = myObject.DataLevels.Length;
            if (length == 0)
            {
                // No data level, so not necessary to create container
                return;
            }
           
            
            string containerName = "Playable Volume container";
            var container = GetContainer(containerName);
            for (int i = 0; i < length; i++)
            {
                var playableVolume =  myObject.DataLevels[i].playableVolume;
                if (playableVolume == null) continue;
                playableVolume.name =  "Playable Volume Level " + i;
                playableVolume.transform.parent = container.transform;
            }
        }
        private void CreateContainerFloorCollision()
        {
            int length = myObject.DataLevels.Length;
            if (length == 0)
            {
                // No data level, so not necessary to create container
                return;
            }

            var container = GetContainer(ContainerNameFloorCollision);
         
            for (int i = 0; i < length; i++)
            {
                var floorCollision =  myObject.DataLevels[i].floorCollision;
                if (floorCollision == null) continue;
                floorCollision.name =  "Floor Collision Level " + i;
                floorCollision.transform.parent = container.transform;
            }
        }
        

        #endregion
        
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
