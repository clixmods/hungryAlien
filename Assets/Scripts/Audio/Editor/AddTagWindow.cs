using UnityEditor;
using UnityEngine;

namespace Audio.Editor
{

    public class AddTagWindow : EditorWindow
    {
        string tagname = "";
        string layername = "";
        private AliasesEditorWindow _previousWindow;
        
        
        public void OnGUI()
        {
            //TagsAndLayers myTarget = (TagsAndLayers)target;

            tagname = EditorGUILayout.TextField("", tagname);
            

            if (tagname == "" || tagname == null)
            {
                GUI.enabled = false;
            }
            else
            {
                GUI.enabled = true;
            }
            if (GUILayout.Button("Create Tag"))
            {
                UnityEditorInternal.InternalEditorUtility.AddTag($"Audio Aliase/{tagname}");
                GetWindow<AliasesEditorWindow>("Audio Aliases Editor").UpdateTagList();
                Close();
            }

            if (GUILayout.Button("Remove Tag"))
            {
              //  UnityEditorInternal.InternalEditorUtility.RemoveTag();
            }

        }
    }


}