using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.AnimatedValues;
using UnityEngine.Windows;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace FXEditor
{
    public class FXEditorWindow : EditorWindow
    {
        private Editor _editor;
        private SerializedObject _selectedSerializeObject;
        private SerializedObject[] _serializedObjects;
        private bool DrawUI;
        
        // List of FX
        private int _indexFXSelected;
        private string[] _listOfFXNames;

        [MenuItem("Clix Addons/FX Editor")]
        public static void Open()
        {
            FXEditorWindow window = GetWindow<FXEditorWindow>("FX Editor");
        }
        private void OnGUI()
        {
            {
                var fxs = GetAllInstances<FXScriptableObject>();
                int length = fxs.Length;
                _serializedObjects = new SerializedObject[length];
                _listOfFXNames = new string[length];
                for (int i = 0; i < length; i++)
                {
                    _serializedObjects[i] = new SerializedObject(fxs[i]);
                    _listOfFXNames[i] = fxs[i].name;
                }

                DrawSidebar();

                _selectedSerializeObject = _serializedObjects[_indexFXSelected];
                
                //using (new GUILayout.HorizontalScope(EditorStyles.whiteLabel,GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(false)))
                {
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(false) , GUILayout.Width(50));
                      
                      
                        //Editor.DrawFoldoutInspector(_selectedSerializeObject.targetObject, ref _editor);
                        Editor.CreateCachedEditor(_selectedSerializeObject.targetObject, null, ref _editor);
                        EditorGUI.indentLevel++;
                        _editor.OnInspectorGUI();
                        
                        //_editor.DrawDefaultInspector();
                        EditorGUILayout.Space();

                        //_editor.
                   EditorGUILayout.EndVertical();
                   
                }

                
            }
        }

       


        void DrawSidebar()
        {
          //  using (new GUILayout.VerticalScope(EditorStyles.whiteLabel))
            {
                EditorGUILayout.LabelField("Filter");

            
                _indexFXSelected = EditorGUILayout.Popup("FX Selected", _indexFXSelected, _listOfFXNames);
                
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Create FXAsset"))
                {
                    CreateNewFX();
                }
                
             
                
            }
        }

       

        private void CreateNewFX()
        {
            var path  = EditorUtility.SaveFilePanelInProject(
                "Save FX Asset",
                $"FX_",
                "asset",
                "asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                UnityEditor.AssetDatabase.CreateAsset(CreateInstance<FXScriptableObject>(), path);
                AssetDatabase.Refresh();
            }

        }

        // Find every instance of a ScriptableObjects
        private static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[]
                guids = AssetDatabase.FindAssets("t:" +
                                                 typeof(T)
                                                     .Name); //FindAssets uses tags check documentation for more info
            int count = guids.Length;
            T[] a = new T[count];
            for (int i = 0; i < count; i++) //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;
        }

       
    }
}