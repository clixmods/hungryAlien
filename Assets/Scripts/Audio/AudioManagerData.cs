using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace AudioAliase
{
    [CreateAssetMenu(fileName = "AudioManager Data", menuName = "Audio/Audio Manager Data", order = 2)]
    public class AudioManagerData : ScriptableObject
    {
        public Aliases[] aliases;
        
        #if UNITY_EDITOR
            private void OnValidate()
            {
                aliases = GetAllInstances();
            }
            private static Aliases[] GetAllInstances()
            {
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(Aliases).Name);  //FindAssets uses tags check documentation for more info
                int count = guids.Length;
                Aliases[] a = new Aliases[count];
                for (int i = 0; i < count; i++)         //probably could get optimized 
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = (Aliases) AssetDatabase.LoadAssetAtPath<Aliases>(path);
                    if(!asset.DontLoad)
                        a[i] = AssetDatabase.LoadAssetAtPath<Aliases>(path);
                }

                return a;

            }
        #endif
       
    }
}