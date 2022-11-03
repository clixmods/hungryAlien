using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(ObjectPhysics)), CanEditMultipleObjects()]
public class ObjectPhysicsEditor : Editor
{
    private Editor _editor;
    private bool _foldSO;
    private ObjectPhysics myTarget;
    private SerializedProperty _serializedProperty;
    private const string PropertyName = "settings";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
         myTarget = (ObjectPhysics) target;
         var so = new SerializedObject(target);
         _serializedProperty = so.FindProperty(PropertyName);
        if (GUILayout.Button("Create new setting"))
        {
            ButtonSaveAsset();
        }

        if (_serializedProperty != null)
        {
            
            // Draw ScriptableObject in the inspector
            _serializedProperty = serializedObject.FindProperty(PropertyName);
            //myTarget.gameObject.name = $"{_serializedProperty.name}";
            _foldSO = EditorGUILayout.InspectorTitlebar(_foldSO, _serializedProperty.objectReferenceValue);
            if (_foldSO)
            {
                CreateCachedEditor(_serializedProperty.objectReferenceValue, null, ref _editor);
                EditorGUI.indentLevel++;
                _editor.OnInspectorGUI();
            }
        }
        // else
        // {
        //     myTarget.gameObject.name = $"ObjectPhysics Setting no setup";
        // }
        
        
    }

    void ButtonSaveAsset()
    {
        //ObjectPhysicsScriptableObject instance = ScriptableObject.CreateInstance<ObjectPhysicsScriptableObject>();
        var path  = EditorUtility.SaveFilePanelInProject(
                "Save aliase file",
                $"objphysetting_",
                "asset",
                "asset");

        if (!string.IsNullOrEmpty(path))
        {
            UnityEditor.AssetDatabase.CreateAsset(CreateInstance<ObjectPhysicsScriptableObject>(), path);
           // var oof = (ObjectPhysicsScriptableObject)AssetDatabase.LoadAssetAtPath(path, typeof(ObjectPhysicsScriptableObject));
            _serializedProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Object>(path);
            AssetDatabase.Refresh();
        }
    }
}
