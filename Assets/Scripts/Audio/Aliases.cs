using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;

namespace AudioAliase
{
    [CreateAssetMenu(fileName = "new_aliases", menuName = "Audio/aliases", order = 1)]
    public class Aliases : ScriptableObject
    {
        public bool DontLoad;
        public AudioMixerGroup defaultMixerGroup;
        public List<Aliase> aliases;
        public Dictionary<string, Queue<Aliase>> aliasesDictionnary;
        private void OnValidate()
        {
            if (DontLoad == true)
                return;
            
            //AudioManager.AddAliases(this);
        }
        private void Awake()
        {
            //AudioManager.AddAliases(this);
        }
        
    }
    
}
