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

            for (int i = 0; i < aliases.Count; i++)
            {
                if (!aliases[i].isInit)
                {
                    aliases[i].isInit = true;
                    if (aliases[i].volume == 0)
                        aliases[i] = new Aliase();
                }

                if (aliases[i].minPitch > aliases[i].maxPitch)
                    aliases[i].minPitch = aliases[i].maxPitch - 0.01f;

                if (aliases[i].MinDistance > aliases[i].MaxDistance)
                    aliases[i].MinDistance = aliases[i].MaxDistance - 0.01f;

                if (aliases[i].MixerGroup == null)
                    aliases[i].MixerGroup = defaultMixerGroup;

                if (!aliases[i].randomizeClips && aliases[i].audio.Length > 1)
                {
                    AudioClip tempClip = aliases[i].audio[0];
                    aliases[i].audio = new AudioClip[1];
                    aliases[i].audio[0] = tempClip;
                }
            }
            AudioManager.AddAliases(this);
        }
        private void Awake()
        {
            AudioManager.AddAliases(this);
        }


    }

}
