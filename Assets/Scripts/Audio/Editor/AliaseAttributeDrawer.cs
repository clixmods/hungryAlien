using System.Collections.Generic;
using AudioAliase;
using UnityEditor;
using UnityEngine;

namespace Audio.Editor
{
    public static class StringAliasExtension
    {
        public static void NullSound(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                str = AudioManager.AliasNameNull;
            }
        }
    }
    
    //TODO : NEED TO OPTIMIZE CAUSE BAD FRAMERATE
    [CustomPropertyDrawer(typeof(AliaseAttribute))]
    public class AliaseAttributeDrawer : PropertyDrawer
    {
        private List<string> nameScenes;
        Aliases[] _aliasesArray;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_aliasesArray == null || _aliasesArray.Length == 0)
            {
                _aliasesArray = GetAllInstances<Aliases>();
                Debug.Log("Aliase get in attribute");
            }
            nameScenes = new List<string>();
            // Index 0 is null name
            nameScenes.Add(AudioManager.AliasNameNull);
            foreach (Aliases asset in _aliasesArray)
            {
                foreach (var VARIABLE in asset.aliases)
                {
                    nameScenes.Add(VARIABLE.name);
                }
            }
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = EditorGUILayout.Popup(property.intValue, nameScenes.ToArray());
                    break;
                case SerializedPropertyType.String:
                    int i = EditorGUI.Popup(position, property.name ,GetIndexFromName(property.stringValue), nameScenes.ToArray());
                    property.stringValue = nameScenes[i];
                    break;
                default :
                    EditorGUILayout.LabelField("Use Scene with Int or String"); 
                    break;
            }
            
            
        } 
        int GetIndexFromName(string strToSearch)
        {
            int length = nameScenes.Count;
            if (string.IsNullOrEmpty(strToSearch))
                return 0;
                
            for (int i = 0; i < length; i++)
            {
                if (nameScenes[i] == strToSearch)
                    return i;
            }
            return 0;
        }
        
        private static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
            int count = guids.Length;
            T[] a = new T[count];
            for (int i = 0; i < count; i++)         //probably could get optimized 
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return a;

        }
    }
}