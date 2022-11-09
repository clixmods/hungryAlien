using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common.Tree;
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
    
    // Add a menu item to create custom GameObjects.
    // Priority 10 ensures it is grouped with the other menu items of the same kind
    // and propagated to the hierarchy dropdown and hierarchy context menus.
    [MenuItem("GameObject/Custom/Gameplay Element/Object Physics", false, 1)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("New Object Physics");
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
        go.AddComponent<ObjectPhysics>();
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
    }

    static bool ValidateTransformIntoCustomGameObject(GameObject targetGameObject)
    {
        // We need to know if the selected GameObject can be transformed to an ObjectPhysics
        if (targetGameObject != null)
        {
            if (targetGameObject.TryGetComponent(out MeshFilter meshFilter)
                && targetGameObject.TryGetComponent(out MeshRenderer meshRenderer) &&
                !targetGameObject.TryGetComponent(out ObjectPhysics objectPhysics))
            {
                return true;
            }
        }

        return false;
    }

    [MenuItem("GameObject/Custom/Gameplay Element/Convert to Object Physics", false, 1)]
    static void TransformIntoCustomGameObject(MenuCommand menuCommand)
    {
        var selectedGameObject = (GameObject) menuCommand.context;
        if(ValidateTransformIntoCustomGameObject(selectedGameObject))
            selectedGameObject.AddComponent<ObjectPhysics>();
    }
}
