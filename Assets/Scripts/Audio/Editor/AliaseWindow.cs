using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AudioAliase.Edit
{


    public class AliaseWindow : ExtendedEditorWindow
    {
        [SerializeField] static Aliases[] aliasesArray = new Aliases[0];
        private Object asset;
        private Editor assetEditor;
        // Add a menu item named "Do Something" to MyMenu in the menu bar.
        [MenuItem("MyMenu/Do Something")]
        static void DoSomething()
        {
            Debug.Log("Doing Something...");
            aliasesArray = GetAllInstances<Aliases>();

            AliaseWindow window = GetWindow<AliaseWindow>("Aliases"); //3
                                                                      //window.serializedObject = new SerializedObject(aliasesArray);
                                                                      //foreach (Aliases asset in aliasesArray)
                                                                      //{
                                                                      //    //GUILayout.BeginHorizontal(); //4
                                                                      //    GUILayout.Label(asset.name, GUILayout.Width(70)); //5
                                                                      //    //this.serializedObject = new SerializedObject(asset);



            //}*

         

        }
        // Find every instance of a ScriptableObjects
        public static T[] GetAllInstances<T>() where T : ScriptableObject
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
        // Define items to be displayed in the Window
        void OnGUI() //1
        {
            EditorGUILayout.HelpBox("Here you can find all aliases files created.", MessageType.Info);
            GUILayout.Label("Aliases file", EditorStyles.boldLabel); //2
            AliaseWindow window = GetWindow<AliaseWindow>("Aliases"); //3

            //color = EditorGUILayout.ColorField("Color", color); //3
            foreach (Aliases asset in aliasesArray)
            {
                //window.serializedObject = new SerializedObject(aliasesArray);
                //window.currentProperty = window.serializedObject.FindProperty("aliases");
                //DrawProperties(window.currentProperty, true);
                ////GUILayout.BeginHorizontal(); //4
                //GUILayout.Label(asset.name, GUILayout.Width(70)); //5
                ////this.serializedObject = new SerializedObject(asset);
                ////serializedObject = new SerializedObject(aliasesArray);
                ////currentProperty = window.serializedObject.FindProperty("aliases");
                ////DrawProperties(currentProperty, true);
                ////DrawProperties(currentProperty, true);
                //if (GUILayout.Button("Open")) //4
                //{
                //   // var window = CreateWindow<AliaseWindow>($"{asset.name} | {asset.GetType().Name}");
                //    window.asset = asset;

                //    window.assetEditor = Editor.CreateEditor(asset);
                //    OpenInspector();
                //    UnityEditor.Selection.activeObject = asset;
                //}
                //List<Aliase> alist = asset.aliases;


            }
            if (GUILayout.Button("Create new one")) //4
            {
                //Colorize();
            }

        }

        void OpenInspector()
        {
            // Retrieve the existing Inspector tab, or create a new one if none is open
            EditorWindow inspectorWindow = GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            // Get the size of the currently window
            Vector2 size = new Vector2(inspectorWindow.position.width, inspectorWindow.position.height);
            if (inspectorWindow == null) inspectorWindow = Instantiate(inspectorWindow);
            // Set min size, and focus the window
            inspectorWindow.minSize = size;
            inspectorWindow.Show();
            inspectorWindow.Focus();

        }
    }
}