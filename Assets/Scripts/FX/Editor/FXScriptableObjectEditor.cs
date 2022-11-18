using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

[CustomEditor(typeof(FXScriptableObject))]
public class FXScriptableObjectEditor : Editor
{
    private FXScriptableObject myTarget;
    private bool _foldInspector;
    private Editor _editorTransform;
    private Editor _editorFX;
    private GameObject _cachedFxPrefab;

    public override void OnInspectorGUI()
    {
        myTarget = (FXScriptableObject) target;
        base.OnInspectorGUI();
        var myParticleSystem = serializedObject.FindProperty("_particleSystem");
        
        if (myTarget._fxPrefab == null)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.whiteLabel))
            {
                if (GUILayout.Button("Create ParticleSystem"))
                {
                    CreateParticleSystemPrefab();
                }

                GUILayout.Label("Or");
                if (GUILayout.Button("Create VisualEffect"))
                {
                    CreateVisualEffectPrefab();
                }
            }

            return;
        }

        if (myTarget._fxPrefab.TryGetComponent<ParticleSystem>(out ParticleSystem fxParticleSystemComponent))
        {
            DrawEditorFromComponent(fxParticleSystemComponent);
        }

        if (myTarget._fxPrefab.TryGetComponent<VisualEffect>(out VisualEffect fxVisualEffectComponent))
        {
            DrawEditorFromComponent(fxVisualEffectComponent);
        }

        if (_cachedFxPrefab != myTarget._fxPrefab || !PrefabStageUtility.GetCurrentPrefabStage())
        {
            string myPath = AssetDatabase.GetAssetPath(myTarget._fxPrefab);
            var previous = Selection.objects;
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(myPath));
            _cachedFxPrefab = myTarget._fxPrefab;
            Selection.objects = previous;
        }
    }

    void DrawEditorFromComponent<T>(T component) where T : Object
    {
        _foldInspector = EditorGUILayout.InspectorTitlebar(_foldInspector, component);
        if (_foldInspector)
        {
            CreateCachedEditor(component, null, ref _editorFX);
            EditorGUI.indentLevel++;
            _editorFX.OnInspectorGUI();
        }
    }

    private void CreateParticleSystemPrefab()
    {
        throw new NotImplementedException();
    }

    private void CreateVisualEffectPrefab()
    {
        throw new NotImplementedException();
    }
}