using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private bool _foldSO;
    private UnityEditor.Editor _editorSO;

    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        var serializedProperty = serializedObject.FindProperty("_data");
        if (serializedProperty.objectReferenceValue == null)
        {
            serializedProperty.objectReferenceValue = Resources.Load<Object>("GameManager Data");
        }

        serializedObject.ApplyModifiedProperties();
        _foldSO = EditorGUILayout.InspectorTitlebar(_foldSO, serializedProperty.objectReferenceValue);
        if (_foldSO)
        {
            CreateCachedEditor(serializedProperty.objectReferenceValue, null, ref _editorSO);
            _editorSO.OnInspectorGUI();
        }
    }
    
}
