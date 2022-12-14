using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common.Tree;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[CustomEditor(typeof(ObjectPhysics),true), CanEditMultipleObjects()]
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

        if (GUILayout.Button("Generate Materials"))
        {
            List<Material> mtlsdick = new List<Material>();
            List<Material> mtls = new List<Material>();
            myTarget.GetComponentInChildren<MeshRenderer>().GetSharedMaterials(mtlsdick);
            foreach (Material mtl in mtlsdick)
            {
                mtls.Add(new Material(mtl));
            }
            myTarget.EditorChangeMtlTransparent(mtls.ToArray());
            List<Material> mtlsOpaque = new List<Material>();
            foreach (var mtl in mtls)
            {
                mtlsOpaque.Add(new Material(mtl));
                
            }
            
            foreach (var mtl in mtlsOpaque)
            {
                mtl.shader = Shader.Find("HDRP/Lit");
                mtl.SetFloat("_SurfaceType",0);
                
            }
           
            myTarget.EditorChangeMtlOpaque(mtlsOpaque.ToArray());
        }
        
        if (GUILayout.Button("Generate Collider"))
        {
            var _baseScale  =  myTarget.transform.localScale;
            var _meshCollider = myTarget.transform.GetComponent<MeshCollider>();
            if (_meshCollider != null && _meshCollider.sharedMesh == null)
            {
                myTarget.transform.localScale = Vector3.one;
                MeshFilter[] meshFilters =myTarget.GetComponentsInChildren<MeshFilter>();
                CombineInstance[] combine = new CombineInstance[meshFilters.Length];
                int index = 0;
                while (index < meshFilters.Length)
                {
                    combine[index].mesh = meshFilters[index].sharedMesh;
                    var bound = new Bounds();
                    var ogpos = meshFilters[index].transform.position; 
                    meshFilters[index].transform.position = Vector3.zero;
                    combine[index].transform = meshFilters[index].transform.localToWorldMatrix  ;
                    bound.size = meshFilters[index].transform.localScale;
                    combine[index].mesh.bounds = bound;
                    meshFilters[index].transform.position = ogpos;
                    index++;
                }
            
                var GeneratedCollider = new Mesh();
                GeneratedCollider.CombineMeshes(combine);
                _meshCollider.sharedMesh = GeneratedCollider;
                myTarget.transform.localScale =_baseScale;
                
            }
        }


        if (_serializedProperty != null)
        {
            
            // Draw ScriptableObject in the inspector
            _serializedProperty = serializedObject.FindProperty(PropertyName);
            //myTarget.gameObject.name = $"{_serializedProperty.name}";
            if(_serializedProperty != null && _serializedProperty.objectReferenceValue != null)
            {
                _foldSO = EditorGUILayout.InspectorTitlebar(_foldSO, _serializedProperty.objectReferenceValue);
                if (_foldSO)
                {
                    CreateCachedEditor(_serializedProperty.objectReferenceValue, null, ref _editor);
                    EditorGUI.indentLevel++;
                    _editor.OnInspectorGUI();
                }
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
