using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Level.Editor
{
    [CustomPropertyDrawer(typeof(LevelAttribute))]
    public class LevelAttributeDrawer : PropertyDrawer
    {
        private List<string> nameLevel;
        private SerializedObject _serializedLevelManager;
        private const string PropertyDataLevelName = "dataLevels";
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            var levelManager = GameObject.FindObjectOfType<LevelManager>();
            if(levelManager == null) base.OnGUI(position, property, label);
            else
            {
                _serializedLevelManager ??= new SerializedObject(levelManager);
                nameLevel = new List<string>();
                var serializedProperty = _serializedLevelManager.FindProperty(PropertyDataLevelName);
                int length = serializedProperty.arraySize;
                for (int i = 0; i < length; i++)
                {
                    var arrayElem = serializedProperty.GetArrayElementAtIndex(i);
                    string name = arrayElem.FindPropertyRelative("name").stringValue;
                    nameLevel.Add($" [{i}] : {name}");
                }

            }
            
            EditorGUI.LabelField(position,"Grabbable at level");
            Rect nextPos = position;
            nextPos.x += 120;
            nextPos.width -= 120;
            property.intValue = EditorGUI.Popup(nextPos,property.intValue, nameLevel.ToArray());
            
        }
    }
}