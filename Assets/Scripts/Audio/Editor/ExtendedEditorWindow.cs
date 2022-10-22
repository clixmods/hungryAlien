using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AudioAliase;

public class ExtendedEditorWindow : EditorWindow
{
    protected List<SerializedObject> serializedObject;    
    protected SerializedProperty currentProperty;
    protected SerializedObject selectedSerializeObject;


    private string selectedPropertyPath;
    protected SerializedProperty selectedProperty;
    protected SerializedProperty currentElemFromArraySelected;
    protected bool showAll = false;

    protected const string AliasesPropertyName = "aliases";
    protected const string AliasesTagCategoryName = "Audio Aliase/";

    // protected void DrawProperties(SerializedProperty prop, bool drawChildren)   
    // {
    //     string lastPropPath = string.Empty;
    //     foreach (SerializedProperty p in prop)
    //     {
    //         if (p.isArray && p.propertyType == SerializedPropertyType.Generic)  
    //         {
    //             
    //             //Debug.Log(p.arraySize);
    //             EditorGUILayout.BeginHorizontal();
    //             if( GUILayout.Button("-"))
    //             {
    //                 //currentProperty.InsertArrayElementAtIndex(currentProperty.arraySize);
    //                // currentProperty.DeleteArrayElementAtIndex()
    //             }   
    //             // Draw the elem part with the arrow
    //             p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName); 
    //             EditorGUILayout.EndHorizontal();
    //
    //             if (p.isExpanded)  
    //             {
    //                 EditorGUI.indentLevel++;   
    //                 DrawProperties(p, drawChildren);  
    //                 EditorGUI.indentLevel--;    
    //             }
    //         }
    //         else
    //         {
    //             if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) { continue; }
    //             lastPropPath = p.propertyPath;
    //              //Debug.Log(lastPropPath);
    //             
    //             
    //              EditorGUILayout.BeginVertical("HelpBox");
    //              if(p.isExpanded )
    //             {
    //                 if( GUILayout.Button("Delete"))
    //                 {
    //
    //                 }
    //             }
    //             // Draw the property on the window, like the inspector 
    //             EditorGUILayout.PropertyField(p, drawChildren);
    //             //Debug.Log(p.name);
    //             EditorGUILayout.EndVertical();
    //         }
    //     }
    // }

    protected void DrawSidebar(SerializedObject prop)
    {
        //foreach (SerializedProperty p in prop)
        //{
        
        EditorGUILayout.BeginHorizontal();

        currentProperty = prop.FindProperty(AliasesPropertyName);
            if (GUILayout.Button($"{prop.targetObject.name} Count : {currentProperty.arraySize}" ))
            {
                //selectedPropertyPath = p.propertyPath;
                selectedSerializeObject = prop;
                showAll = false;
            }
        
        // if (!string.IsNullOrEmpty(selectedPropertyPath))
        // {
        //
        //     //selectedProperty = 
        // }
         EditorGUILayout.EndHorizontal();
    }

    protected void DrawField(string propName, bool relative)
    {
        if( relative && currentElemFromArraySelected != null)
        {
            SerializedProperty sP = currentElemFromArraySelected.FindPropertyRelative(propName);
            if (propName == "isLooping")
            {

            }
            EditorGUILayout.PropertyField(sP, true);
            //Debug.Log(sP.name); // On arrive à leur la valeur du bool cest cool
        }
        else if (selectedSerializeObject != null)
        {
            EditorGUILayout.PropertyField(selectedSerializeObject.FindProperty(propName),true);
        }
    }

    protected virtual void Apply()
    {
        //serializedObject.ApplyModifiedProperties();
    }
}
