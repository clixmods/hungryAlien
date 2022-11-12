using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace AudioAliase
{
      [System.Serializable]
    public class Aliase
    {
        public string name;
        public string description;

        public string Tag;

        public AudioMixerGroup MixerGroup;
        public AudioClip[] audio;

        public SoundType soundType = SoundType.Root;

        [Tooltip("Take a random clip, each time the aliase is played")]
        public bool randomizeClips;
        [Tooltip("Index for the randomizeClips")]
        int _indexRandomize = 0;

        [Tooltip("Secondary aliase played with the primary aliase")]
        public string Secondary;

        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;
        [Range(0, 256)]
        public float priority;
        // TODO : Do a random range
        [Range(0, 1)]
        public float volume = 0.8f; // Obsoletes
        public float minVolume = 0.8f;
        public float maxVolume = 0.8f;

        public bool isLooping;
        public string startAliase;
        public string endAliase;


        [Range(-3, 3)]
        public float minPitch = 1f;
        [Range(-3, 3)]
        public float maxPitch = 1.01f;
        [Range(-1, 1)]
        public float stereoPan = 0;
        [Range(0, 1)]
        public float spatialBlend = 0;

        [Range(0, 1.1f)]
        public float reverbZoneMix = 1;
        [Header("3D Sound Settings")]
        [Range(0, 5)]
        public float dopplerLevel = 1;
        [Range(0, 360)]
        public float Spread = 1;
        [Range(0, 10000)]
        public float MinDistance = 1;
        [Range(0, 10000)]
        public float MaxDistance = 500;
        public AudioRolloffMode CurveType = AudioRolloffMode.Logarithmic;
        public AnimationCurve distanceCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });

        [Header("Subtitle")]
        public string Text;
        public float customDuration;

        public bool isInit;
        public bool isPlaceholder = true;

        public AudioClip Audio
        {
            get
            {
                if (randomizeClips)
                {
                    _indexRandomize = Random.Range(0, audio.Length);
                    return audio[_indexRandomize];
                }
                return audio[0];
            }
        }

        // Give default value for a new aliase
        // We override the default constructor, because Unity doesnt give default value when we initialize variable
        // will be fix by unity in the future...
        public Aliase()
        {
            name = "newAliase";
            volume = 0.8f;
            minVolume = 0.8f;
            maxVolume = 0.8f;
            minPitch = 1f;
            maxPitch = 1.01f;
            reverbZoneMix = 1;
            dopplerLevel = 1;
            Spread = 1;
            MinDistance = 1;
            MaxDistance = 500;
        }

    }

}