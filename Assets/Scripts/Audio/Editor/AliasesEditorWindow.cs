using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.Playables;

using System.Reflection;
using UnityEditor.AnimatedValues;
using UnityEngine.Windows;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace AudioAliase.Edit
{

    public class AliasesEditorWindow : ExtendedEditorWindow
    {
        Vector2 scrollPos;
        private static Aliases[] _aliasesArray = new Aliases[0];

        private string _searchValue;

        bool _fold;
        Transform selectedTransform;

        private List<string> _options;
        /// <summary>
        /// List of all aliases created in the project
        /// </summary>
        private List<string> _aliases;
        /// <summary>
        /// List of all aliases created in the project for selection with none value
        /// </summary>
        private List<string> _aliasesOptions;
        // Constant string
        private const string TAG_MNGR_ASSET = "ProjectSettings/TagManager.asset";
        private const string ASSET_ALREADY_EXIST = "An asset exists already with the name, change it";
        int _numberTags;    
        private string[] _tags;


        int _selected = 0;

        int _selectedTagIndex = 0;

        string newAliaseName = "New aliases file name";

        //FILTER
        bool showOnlyPlaceHolder;

        bool show3DSettings;
        private string _message;

        private AudioSource _audioSource;
        // Method executed when we open the window
        [MenuItem("Clix Addons/Audio Aliases Editor")]
        public static void Open()
        {
            AliasesEditorWindow window = GetWindow<AliasesEditorWindow>("Audio Aliases Editor");
            window.UpdateAliasesFileList();
            window.UpdateAliasesList();
            window.UpdateTagList();
            GameObject oof = new GameObject();
            
            window._audioSource = oof.AddComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            DestroyImmediate(_audioSource.gameObject);
        }

        public void UpdateTagList()
        {
            _tags = GetTags();
        }
        public void SetupAudioSource(Aliase aliase)
        {
            _audioSource.clip = aliase.Audio;
            _audioSource.volume = Random.Range(aliase.minVolume, aliase.maxVolume);
            _audioSource.loop = aliase.isLooping;
            _audioSource.pitch = Random.Range(aliase.minPitch, aliase.maxPitch);

            _audioSource.spatialBlend = aliase.spatialBlend;
            if (aliase.MixerGroup != null)
                _audioSource.outputAudioMixerGroup = aliase.MixerGroup;

            switch (aliase.CurveType)
            {
                case AudioRolloffMode.Logarithmic:
                case AudioRolloffMode.Linear:
                    _audioSource.rolloffMode = aliase.CurveType;
                    break;
                case AudioRolloffMode.Custom:
                    _audioSource.rolloffMode = aliase.CurveType;
                    _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, aliase.distanceCurve);
                    break;

            }
        }
        void PlaySound()
        {
            //_audioSource.Play();
            //AudioManager.PlaySoundAtPosition(, Vector3.zero);
        }
        void UpdateAliasesFileList()
        {
            serializedObject = new List<SerializedObject>();
            // We get all aliases available in the project, to convert them into serializedObject
            _aliasesArray = GetAllInstances<Aliases>();
            foreach (Aliases asset in _aliasesArray)
            {
               
                    var newSerializedObject = new SerializedObject(asset);
                    serializedObject.Add(newSerializedObject);
                  
            }
         
            selectedSerializeObject = null;
            Repaint();
        }
        private void DrawFilter()
        {

        }
        // This method is executed each frame
        private void OnGUI()
        {
            // Verification before draw the window
            if (serializedObject == null)
            {
                Debug.Log("Warning : Aliase Editor have no serializedObject");
                UpdateAliasesFileList();
                return;
            }
            // We draw the visual of the window
            // First Begin horizontal to draw : left to right
            using (new GUILayout.HorizontalScope())
            {
             
                DrawLeftPanel();
                DrawMiddlePanel();
                DrawRightPanel();
            }
     
            Apply();
        }

        /// <summary>
        /// This method will update the list of aliases, need to be called when its required
        /// </summary>
        private void UpdateAliasesList()
        {
            
            _aliases = new List<string>();
            _aliasesOptions = new List<string>();
            _aliasesOptions.Add("None");
            foreach (SerializedObject o in serializedObject)
            {
                var aliasesListProp = o.FindProperty(AliasesPropertyName);
                foreach (SerializedProperty p in aliasesListProp)
                {
                    _aliases.Add(p.FindPropertyRelative("name").stringValue);
                    _aliasesOptions.Add(p.FindPropertyRelative("name").stringValue);
                }
            }
            Debug.Log(_aliasesOptions.Count);
            
            Apply();
            Repaint();
        }
        void DrawMiddlePanel()
        {
            // Middle part
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(150), GUILayout.MaxWidth(750), GUILayout.ExpandHeight(true));
            // Allow the possibility to use a scrollbar to navigate
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
            
            EditorGUILayout.LabelField("Search");
            _searchValue = EditorGUILayout.TextField(_searchValue);
            
            // if a Aliase file is selected, go draw all properties
            if (selectedSerializeObject != null)
            {
                // Among serializeObjects, we search aliases list for the selected 
                if(showAll)
                {
                    for (int i = 0; i < serializedObject.Count; i++)
                    {
                        currentProperty = serializedObject[i].FindProperty(AliasesPropertyName);
                        DrawTheShit();
                    }
                    // foreach(SerializedObject soee in serializedObject)
                    // {
                    //     
                    // }
                }
                else
                {
                    currentProperty = selectedSerializeObject.FindProperty(AliasesPropertyName);
                    DrawTheShit();
                    if (GUILayout.Button("Create aliase"))
                    {
                        currentProperty.InsertArrayElementAtIndex(currentProperty.arraySize);
                        ApplyAliaseDefaultValue(currentProperty.GetArrayElementAtIndex(currentProperty.arraySize-1));
                        UpdateAliasesList();
                        return;
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Please select a aliases list", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ApplyAliaseDefaultValue(SerializedProperty serializedProperty)
        {
            serializedProperty.FindPropertyRelative("minPitch").floatValue = 1;
            serializedProperty.FindPropertyRelative("maxPitch").floatValue = 1.01f;
            serializedProperty.FindPropertyRelative("MinDistance").floatValue = 0;
            serializedProperty.FindPropertyRelative("MaxDistance").floatValue = 500;
            serializedProperty.FindPropertyRelative("minVolume").floatValue = 0.8f;
            serializedProperty.FindPropertyRelative("maxVolume").floatValue = 0.8f;
            serializedProperty.FindPropertyRelative("reverbZoneMix").floatValue = 1;
            serializedProperty.FindPropertyRelative("dopplerLevel").floatValue = 1;
            serializedProperty.FindPropertyRelative("Spread").floatValue = 1;
            serializedProperty.FindPropertyRelative("isPlaceholder").boolValue = true;
        }

        void DrawTheShit()
        {
            // We draw the content of aliases    
            // Navigate between each elem of the list aliases
            //foreach (SerializedProperty p in currentProperty)
            for (int i = 0; i < currentProperty.arraySize; i++)
            {
               SerializedProperty p = currentProperty.GetArrayElementAtIndex(i);
               currentElemFromArraySelected = p;
                if ( !String.IsNullOrEmpty(_searchValue) && !currentElemFromArraySelected.FindPropertyRelative("name").stringValue.Contains(_searchValue))
                {
                    continue;
                }
                    // check filter
                if (showOnlyPlaceHolder && !currentElemFromArraySelected.FindPropertyRelative("isPlaceholder").boolValue)
                {
                    continue;
                }

                if ( _selectedTagIndex != 0 && _tags[_selectedTagIndex] != currentElemFromArraySelected.FindPropertyRelative("Tag").stringValue)
                {
                    continue;
                }
                
                EditorGUILayout.BeginVertical("HelpBox");
                
                GUIContent spatialRightLabel = EditorGUIUtility.TrTextContent("-","Remove the shi");

                // If I understand correctly, FindPropertyRelative try to find the property declared into arg,
                // Maybe a way to check property inside the elem of the array etc 
                // _options.Add(currentElemFromArraySelected.FindPropertyRelative("name").stringValue);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(spatialRightLabel, GUILayout.Width(30)))
                {
                    var title = "Delete the selection";
                    _message = "Are you sure you want to delete?";
                    var ok = "Delete";
                    var cancel = "No";
                    if( EditorUtility.DisplayDialog(title, _message, ok, cancel))
                    {
                        
                    }
                }
                // Draw the elem part with the arrow
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                EditorGUILayout.EndHorizontal();

                if (  p.isExpanded )
                {
                    EditorGUI.indentLevel++;
                    DrawSelectedPanel();
                    EditorGUI.indentLevel--;
                }
        
                EditorGUILayout.EndVertical();

            }
        }

        void DrawLeftPanel()
        {
            // Left side of the window:
            // Shows every aliases asset with options

            using (new GUILayout.VerticalScope(EditorStyles.whiteLabel))
            {
                //EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
                // Filter part
                // a text, usefull if we want give description to some properties
                EditorGUILayout.LabelField("Filter");
                // A checkbox for the bool showOnlyPlaceholder
                showOnlyPlaceHolder = EditorGUILayout.Toggle("Show only placeholder", showOnlyPlaceHolder);
                _selectedTagIndex = EditorGUILayout.Popup("Tag", _selectedTagIndex, _tags);

                // Aliases part
                if (EditorGUILayout.BeginFadeGroup(1))
                {
                    EditorGUILayout.LabelField("Aliases Files");
                }
          
               
                // We draw a sidebar for each file converted into SerializedObject
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($" All Count : {_aliases.Count}"))
                {
                    showAll = true;
                }

                EditorGUILayout.EndHorizontal();
                foreach (SerializedObject prop in serializedObject)
                {
                    DrawSidebar(prop);
                }
                EditorGUILayout.EndFadeGroup();
                // Begin to draw a line of button after the list of aliases file
              

                EditorGUILayout.Space(10);
                if (GUILayout.Button("Create new aliases file"))
                {
                    CreateNewAliasesFile();
                    
                    return;
                }
                
            } 
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            _selected = EditorGUILayout.Popup("Sound", _selected, _aliasesOptions.ToArray());
            if (GUILayout.Button("Play sound"))
            {
                string aliaseName = _aliasesOptions[_selected];
                //AudioManager.GetSoundByAliase(aliaseName, out Aliase dick);
                //SetupAudioSource(dick);
                AudioManager.PlaySoundAtPosition(aliaseName, Camera.main.transform.position);
                //PlaySound();
                this.ShowNotification(new GUIContent($"Sound {aliaseName} played"));
            }

            EditorGUILayout.FloatField(0.5f);
            if (GUILayout.Button("Play in loop"))
            {
            }

            EditorGUILayout.EndVertical();
        }

        private string[] GetTags()
        {
            _numberTags = UnityEditorInternal.InternalEditorUtility.tags.Length + 2;
            
            string[] temptagarray = new string[_numberTags];
            int newSize = 0;
            temptagarray[newSize] = "None";
            newSize++;
            for (int i = 1; i < _numberTags - 1; i++)
            {
                string tagSelected = UnityEditorInternal.InternalEditorUtility.tags[i - 1];
                if (tagSelected.Contains(AliasesTagCategoryName))
                {
                    temptagarray[newSize] = tagSelected;
                    newSize++;
                }
            }
            newSize++;
            string[] tagsAll = new string[newSize];
            temptagarray[newSize - 1] = "Add tag..";
            for (int i = 0; i < newSize; i++)
            {
                tagsAll[i] = temptagarray[i];
            }
            return tagsAll;
        }

        private int GetIndexFromNameTag()
        {
            string lastString = _tags[_tags.Length - 1];
            for (int i = 0; i < _tags.Length; i++)
            {
                var tagProp = currentElemFromArraySelected.FindPropertyRelative("Tag");
                string valueFromProperty = tagProp.stringValue;
                if(valueFromProperty == lastString)
                {
                    tagProp.stringValue = string.Empty;
                }

                if (_tags[i] == tagProp.stringValue)
                    return i;
            }


            return 0;
        }
        private void DrawSelectedPanel()
        {
            DrawField("name", true);
            DrawField("description", true);
            int selectedTag = GetIndexFromNameTag();
            selectedTag = EditorGUILayout.Popup("Tag", selectedTag, _tags);
            if (selectedTag == _tags.Length - 1 ) // Is Add Tag field
            {
                 EditorWindow.GetWindow<AddTagWindow>();
            }
            else
            {
                var tagProp = currentElemFromArraySelected.FindPropertyRelative("Tag");
                if (selectedTag == 0)
                    tagProp.stringValue = string.Empty;
                else
                    tagProp.stringValue = _tags[selectedTag];
            }
            
            DrawField("MixerGroup", true);
            DrawField("audio", true);
            DrawField("randomizeClips", true);
            int GetIndexFromName(string strToSearch)
            {
                for (int i = 0; i < _aliasesOptions.Count; i++)
                {
                    if (_aliasesOptions[i] == strToSearch)
                        return i;
                }
                return 0;
            }
            var secondaryProp = currentElemFromArraySelected.FindPropertyRelative("Secondary");
            int selectedAll = GetIndexFromName(secondaryProp.stringValue);
            selectedAll = EditorGUILayout.Popup("Secondary", selectedAll, _aliasesOptions.ToArray());
           
            if (selectedAll == 0)
                secondaryProp.stringValue = string.Empty;
            else
                secondaryProp.stringValue = _aliasesOptions[selectedAll];

            
            DrawField("bypassEffects", true);
            DrawField("bypassListenerEffects", true);
            DrawField("bypassReverbZones", true);
            DrawField("priority", true);

            // draw volume field
            SerializedProperty minVolume = currentElemFromArraySelected.FindPropertyRelative("minVolume");
            SerializedProperty maxVolume = currentElemFromArraySelected.FindPropertyRelative("maxVolume");
            float minVolumeValue = minVolume.floatValue;
            float maxVolumeValue = maxVolume.floatValue;
            
            EditorGUILayout.PrefixLabel("Volume");
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            float widthLabel = 75;
            minVolumeValue = EditorGUILayout.FloatField(minVolumeValue, GUILayout.Width(widthLabel));
            EditorGUILayout.MinMaxSlider(ref minVolumeValue, ref maxVolumeValue, 0, 1);
            maxVolumeValue = EditorGUILayout.FloatField(maxVolumeValue, GUILayout.Width(widthLabel));
            EditorGUILayout.EndHorizontal();
            minVolume.floatValue = minVolumeValue;
            maxVolume.floatValue = maxVolumeValue;
            
            SerializedProperty sP = currentElemFromArraySelected.FindPropertyRelative("isLooping");
            sP.boolValue = EditorGUILayout.BeginToggleGroup("Is Looping", sP.boolValue);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
           // DrawField("startAliase", true);
           // DrawField("endAliase", true);
            
            var startProp = currentElemFromArraySelected.FindPropertyRelative("startAliase");
            int selectedStartAll = GetIndexFromName(startProp.stringValue);
            selectedStartAll = EditorGUILayout.Popup("Start Aliase", selectedStartAll, _aliasesOptions.ToArray());
           
            if (selectedStartAll == 0)
                startProp.stringValue = string.Empty;
            else
                startProp.stringValue = _aliasesOptions[selectedStartAll];
            
            var endProp = currentElemFromArraySelected.FindPropertyRelative("endAliase");
            int selectedendAll = GetIndexFromName(endProp.stringValue);
            selectedendAll = EditorGUILayout.Popup("End Aliase", selectedendAll, _aliasesOptions.ToArray());
           
            if (selectedendAll == 0)
                endProp.stringValue = string.Empty;
            else
                endProp.stringValue = _aliasesOptions[selectedendAll];
            
            EditorGUILayout.EndToggleGroup();

            //DrawField("isLooping", true);
            //DrawField("startAliase", true);
            //DrawField("endAliase", true);
            SerializedProperty minPitch = currentElemFromArraySelected.FindPropertyRelative("minPitch");
            SerializedProperty maxPitch = currentElemFromArraySelected.FindPropertyRelative("maxPitch");
            float minPitchValue = minPitch.floatValue;
            float maxPitchValue = maxPitch.floatValue;

            //EditorGUIUtility.SliderLabels //.SetLabels(Styles.priorityLeftLabel, Styles.priorityRightLabel);
            EditorGUILayout.PrefixLabel("Pitch");
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            minPitchValue = EditorGUILayout.FloatField(minPitchValue, GUILayout.Width(widthLabel));
            EditorGUILayout.MinMaxSlider(ref minPitchValue, ref maxPitchValue, -3, 3);
            maxPitchValue = EditorGUILayout.FloatField(maxPitchValue, GUILayout.Width(widthLabel));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Min");
            EditorGUILayout.LabelField("Max");
            EditorGUILayout.EndHorizontal();
            minPitch.floatValue = minPitchValue;
            maxPitch.floatValue = maxPitchValue;

            
            DrawField("stereoPan", true);
            DrawField("spatialBlend", true);
            DrawField("reverbZoneMix", true);
            ////////////////
            ///// Draw the elem part with the arrow
            EditorGUILayout.BeginVertical("box");
            show3DSettings = EditorGUILayout.Foldout(show3DSettings, "3D Sound Settings");
            if (show3DSettings)
            {
                DrawField("dopplerLevel", true);
                DrawField("Spread", true);
                DrawField("MinDistance", true);
                DrawField("MaxDistance", true);

                SerializedProperty minDistance = currentElemFromArraySelected.FindPropertyRelative("MinDistance");
                SerializedProperty maxDistance = currentElemFromArraySelected.FindPropertyRelative("MaxDistance");
                float minDistanceValue = minDistance.floatValue;
                float maxDistanceValue = maxDistance.floatValue;
                EditorGUILayout.PrefixLabel("Min and Max distance");
                EditorGUILayout.MinMaxSlider(ref minDistanceValue, ref maxDistanceValue, 0, 10000);
                minDistance.floatValue = minDistanceValue;
                maxDistance.floatValue = maxDistanceValue;

                DrawField("CurveType", true);
                DrawField("distanceCurve", true);
                //
            }
            EditorGUILayout.EndVertical();

            DrawField("Text", true);
            DrawField("customDuration", true);

            DrawField("isInit", true);
            DrawField("isPlaceholder", true);
            
            // private void DrawAudioSource3DSettings() {
            //     audioSource.minDistance = minDistance.floatValue;
            //     audioSource.maxDistance = maxDistance.floatValue;
            //     audioSource.rolloffMode = (AudioRolloffMode) rollOffMode.enumValueIndex;
            //
            //     audioSourceEditor.serializedObject.Update();
            // var audioSourceEditor = Editor.CreateEditor(_audioSource); 
            // System.Reflection.MethodInfo methodDrawAudio3D = audioSourceEditor.GetType().GetMethod("Audio3DGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            //
            // methodDrawAudio3D.Invoke(audioSourceEditor, null);

        // EditorGUIUtility.sliderLabels.SetLabels(AudioSourceInspector.Styles.panLeftLabel, AudioSourceInspector.Styles.panRightLabel);
           // var oof = new EditorGUIUtility();
            //var slider = typeof(EditorGUIUtility.).GetProperties(BindingFlags.NonPublic );
            // Debug.Log(slider.Length);

           // var oofa = slider.GetType().GetMethod("SetLabels"); //.Invoke("SetLabels",BindingFlags.NonPublic | BindingFlags.Instance);
            
            /*
                var tryParseMethod = typeof(int).GetMethod(nameof(int.TryParse),
                                                           new[]
                                                           {
                                                               typeof(string),
                                                               typeof(int).MakeByRefType()
                                                           });

                // use it
                var parameters = new object[] { "1", null };
                var success = (bool)tryParseMethod.Invoke(null, parameters);
                var result = (int)parameters[1];             *
             * *
             */
             
            
            //oofa.Invoke(oofa);
            //EditorGUIUtility.sliderLabels.SetLabels(
            //
            // MethodInfo methodDrawAudio3D = audioSourceEditor.GetType().GetMethod("Audio3DGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            //
            // methodDrawAudio3D.Invoke(audioSourceEditor, null);
            //
            // audioSourceEditor.serializedObject.ApplyModifiedProperties();
            //
            // minDistance.floatValue = audioSource.minDistance;
            // maxDistance.floatValue = audioSource.maxDistance;
            // rollOffMode.enumValueIndex = (int)audioSource.rolloffMode;
            // doppler.floatValue = audioSource.dopplerLevel;
            // spread.floatValue = audioSource.spread;
            // customFalloffCurve.animationCurveValue = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
            //}

        }



        protected override void Apply()
        {
            
            foreach (SerializedObject asset in serializedObject)
            {   
                asset.ApplyModifiedProperties();
            }
        }

        // Find every instance of a ScriptableObjects
        private static T[] GetAllInstances<T>() where T : ScriptableObject
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

        string IsValidName(string nameToValidate)
        {
            return nameToValidate;
            foreach (Aliases asset in _aliasesArray)
            {
                //s if (nameToValidate == aliasesArray.)
            }
        }
        /// <summary>
        /// Create a new file to contain aliase list,
        /// need to be created in the folder where the previous file is created
        /// </summary>
        void CreateNewAliasesFile()
        {
            Aliases instance = ScriptableObject.CreateInstance<Aliases>();
            string path = $"Assets/{IsValidName(newAliaseName)}.asset";
            
            var oof  = EditorUtility.SaveFilePanelInProject(
                "Save aliase file",
                $"oof",
                "asset",
                "asset");
            
            if (!string.IsNullOrEmpty(oof))
            {
                UnityEditor.AssetDatabase.CreateAsset(CreateInstance<Aliases>(), oof);
                AssetDatabase.Refresh();
            }
            
            UpdateAliasesFileList(); // Update the left panel
        }

        // We will controls how to draw each property from each element of the list
        void DrawAliaseField()
        {

        }
    }


}

/*
 
 groupEnabled = EditorGUILayout.BeginToggleGroup ("Optional Settings", groupEnabled);
            myBool = EditorGUILayout.Toggle ("Toggle", myBool);
            myFloat = EditorGUILayout.Slider ("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup ();
 */