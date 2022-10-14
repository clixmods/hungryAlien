using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AudioAliase
{
    [CreateAssetMenu(fileName = "AudioManager Data", menuName = "Audio/Audio Manager Data", order = 2)]

    public class AudioManagerData : ScriptableObject
    {

        public List<Aliase> aliases;
        public Dictionary<string, Queue<Aliase>> aliasesDictionnary;

        public List<string> tagsAliases;

        private void OnValidate()
        {
            tagsAliases = UnityEditorInternal.InternalEditorUtility.tags.ToList();
            //UnityEditorInternal.InternalEditorUtility.AddTag();
        }

       

    }
}