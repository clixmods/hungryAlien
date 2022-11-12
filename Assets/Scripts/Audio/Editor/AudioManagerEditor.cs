using AudioAliase;
using UnityEditor;

namespace Audio.Editor
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor
    {
        private bool _foldSO;
        private UnityEditor.Editor _editorSO;
        
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            var serializedProperty = serializedObject.FindProperty("_audioManagerData");
            _foldSO = EditorGUILayout.InspectorTitlebar(_foldSO, serializedProperty.objectReferenceValue);
            if (_foldSO)
            {
                CreateCachedEditor(serializedProperty.objectReferenceValue, null, ref _editorSO);
                EditorGUI.indentLevel++;
                _editorSO.OnInspectorGUI();
            }
        }
    }
}