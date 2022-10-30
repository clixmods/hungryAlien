using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(ObjectPhysics)), CanEditMultipleObjects()]
public class ObjectPhysicsEditor : Editor
{
    private Editor _editor;
    private bool _foldSO;
    private ObjectPhysics myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
         myTarget = (ObjectPhysics) target;
         
        if (GUILayout.Button("Create new setting"))
        {
            ButtonSaveAsset();
        }

        if (myTarget._settings != null)
        {
            
            // Draw ScriptableObject in the inspector
            var myAsset = serializedObject.FindProperty("_settings");
            myTarget.gameObject.name = $"{myTarget._settings.name}";
            _foldSO = EditorGUILayout.InspectorTitlebar(_foldSO, myTarget._settings);
            if (_foldSO)
            {
                CreateCachedEditor(myAsset.objectReferenceValue, null, ref _editor);
                EditorGUI.indentLevel++;
                _editor.OnInspectorGUI();
            }
        }
        else
        {
            myTarget.gameObject.name = $"ObjectPhysics Setting no setup";
        }
        
        
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
            myTarget._settings = (ObjectPhysicsScriptableObject)AssetDatabase.LoadAssetAtPath(path, typeof(ObjectPhysicsScriptableObject));
            AssetDatabase.Refresh();
        }
    }
}
