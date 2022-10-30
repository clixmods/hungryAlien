using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;


namespace AudioAliase
{
    public enum SoundType
    {
        Start,
        Root,
        End,
    }
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

    //[RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        private static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("AudioManager").AddComponent<AudioManager>();
                }

                return _instance;
            }
            set { _instance = value; }
        }
        

        public static Aliases[] aliasesArray = new Aliases[0];
        [SerializeField] private List<AudioPlayer> _audioSource;

        [SerializeField] int audioSourcePoolSize = 32; // 32 is a good start
        [Header("Debug")]
        [SerializeField] Aliases[] TableAliasesLoaded = new Aliases[0];

        bool _isPaused;
        

        public bool IsPaused
        {
            set { _isPaused = value; }
        }
        // Add aliases to load
        public static void AddAliases(Aliases newAliases)
        {
            // Check if not already exist
            for (int i = 0; i < aliasesArray.Length; i++)
            {
                if (aliasesArray[i] == newAliases)
                {
                    Debug.Log("AudioManager : L'aliases a déja était ajouté");
                    return;
                }
            }
            // We can add it
            Aliases[] TempAliasesArray = new Aliases[aliasesArray.Length + 1];
            for (int i = 0; i < aliasesArray.Length; i++)
            {
                TempAliasesArray[i] = aliasesArray[i];
            }
            TempAliasesArray[TempAliasesArray.Length - 1] = newAliases;
            aliasesArray = TempAliasesArray;
            Debug.Log("AudioManager : Aliases added");
        }

        void Awake()
        {
            Instance = this;
            InitAudioSources();
        }

        void InitAudioSources()
        {
            _audioSource = new List<AudioPlayer>();
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                GameObject newAudioSource = new GameObject("Audio Source " + i);
                newAudioSource.transform.SetParent(transform);
                AudioPlayer audioS = newAudioSource.AddComponent<AudioPlayer>();
                Instance._audioSource.Add(audioS);
                newAudioSource.SetActive(false);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
            if (TableAliasesLoaded != aliasesArray)
                TableAliasesLoaded = aliasesArray;

            if (_isPaused)
            {
                PauseAllAudio();
            }
            else
            {
                 UnPauseAllAudio();
                //DisableInusedAudioSource();
            }

        }

        void DisableInusedAudioSource()
        {
            foreach (AudioPlayer aS in _audioSource)
            {
                if (!aS.IsUsable)
                {
                    aS.gameObject.SetActive(false);
                }
            }
        }

        public static bool GetSoundByAliase(string name, out Aliase alias)
        {
            alias = null;
            for (int i = 0; i < aliasesArray.Length; i++)
            {
                foreach (Aliase tempalias in aliasesArray[i].aliases)
                {
                    if (tempalias.name == name)
                    {
                        alias = tempalias;

                    }
                }
            }

            if (alias != null && alias.audio.Length == 0)
            {
                Debug.LogError("AudioManager : Aliase: " + name + " contains no sounds.");
                return false;
            }

            if (alias == null)
            {
                Debug.LogWarning("AudioManager : Aliase: " + name + " not found.");
                return false;
            }

            return true;
            //return alias;
        }

        static AudioSource GetAudioSource()
        {
            foreach (AudioPlayer aS in Instance._audioSource)
            {
                if (!aS.Source.isPlaying)
                {
                    return aS.Source;
                }
            }
            return null;
        }

        internal static bool GetAudioPlayer(out AudioPlayer audioPlayer)
        {
            audioPlayer = null;
           
            foreach (AudioPlayer aS in Instance._audioSource)
            {
                if (aS.IsUsable)
                {
                    audioPlayer = aS;
                    return true;
                }
            }
            Debug.LogWarning($"AudioManager : Limits exceded for _audioSource, maybe you need to increase your audioSourcePoolSize (Size = {Instance.audioSourcePoolSize})");
            return false;
        }
        /// <summary>
        /// Play multiple sounds at desired position
        /// </summary>
        /// <param name="aliasesName"></param>
        /// <param name="position"></param>
        /// <param name="waitFinish"></param>
        private static void PlaySoundsAtPosition(string[] aliasesName, Vector3 position, bool waitFinish = true)
        {
            if (waitFinish)
            {
                if (GetAudioPlayer(out AudioPlayer audioPlayer))
                {
                    List<Aliase> aliases = new List<Aliase>();
                    for (int i = 0; i < aliasesName.Length; i++)
                    {
                        if (GetSoundByAliase(aliasesName[i], out Aliase aliase))
                        {
                            //aliases.Add(aliase);
                            audioPlayer._clips.Enqueue(aliase);
                        }
                    }
                    audioPlayer.gameObject.transform.position = position;
                    audioPlayer.gameObject.SetActive(true);
                }

                return;
            }

            for (int i = 0; i < aliasesName.Length; i++)
            {
                PlaySoundAtPosition(aliasesName[i], position);
            }

        }

        public static void PauseAllAudio()
        {
            foreach (AudioPlayer aS in Instance._audioSource)
            {
                if (aS.Source.isPlaying)
                {
                    aS.Source.Pause();
                }
            }
        }
        public static void UnPauseAllAudio()
        {
            foreach (AudioPlayer aS in Instance._audioSource)
            {
                //if(audio.UnPause)
                {
                    aS.Source.UnPause();
                }
            }
        }

        static bool IsValidAliase()
        {

            return false;
        }

        void AliaseIsValid()
        {
            
        }
        public static Aliase PlaySoundAtPosition(string aliaseName, Vector3 position)
        {
            if (string.IsNullOrEmpty(aliaseName))
            {
                Debug.LogError("AudioManager : Un son a voulu être jouer sans d'aliaseName, il faut en assigné un dans le script qui a exécuté la function");
                return null;
            }
            if(GetSoundByAliase(aliaseName, out Aliase clip) && GetAudioPlayer(out AudioPlayer audioPlayer))
            {
                audioPlayer.gameObject.transform.position = position;
                audioPlayer.gameObject.SetActive(true);
                audioPlayer.Play(clip);

                if (clip.isPlaceholder)
                {
                    Debug.LogWarning("Un son placeholder a été jouer, il faut le changer , nom de l'aliase " + aliaseName);
                }

                if (clip.Secondary != System.String.Empty)
                {
                    PlaySoundAtPosition(clip.Secondary, position);
                }

                return clip;
            }

            return null;
        }
        /// <summary>
        /// Play a loop sound on the desired transform
        /// </summary>
        /// <param name="aliaseName"></param>
        /// <param name="transformToTarget">transform to follow</param>
        /// <param name="audioPlayerLoop"> A ref to AudioPlayer, it can be used with the method StopLoopSound</param>
        public static void PlayLoopSound(string aliaseName, Transform transformToTarget, ref AudioPlayer audioPlayerLoop)
        {
            PlayLoopSound(aliaseName, transformToTarget.position, ref audioPlayerLoop);
            audioPlayerLoop.SetTransformToFollow(transformToTarget);
        }
        /// <summary>
        /// Play a loop sound at the desired position
        /// </summary>
        /// <param name="aliaseName"></param>
        /// <param name="position"> The position of the loop sound</param>
        /// <param name="audioPlayerLoop"> A ref to AudioPlayer, it can be used with the method StopLoopSound</param>
        public static void PlayLoopSound(string aliaseName, Vector3 position, ref AudioPlayer audioPlayerLoop )
        {
            if (audioPlayerLoop != null)
            {
                Debug.Log("PlayLoop already played");
                return;
            }
            if (string.IsNullOrEmpty(aliaseName))
            {
                //Debug.LogError( );
                throw new InvalidAliasesException("AudioManager : No specified aliases");
                //return;
            }
            if (!GetSoundByAliase(aliaseName, out Aliase clip))
            {
                return ;
            }
            if (!GetAudioPlayer(out AudioPlayer audioPlayer))
            {
                Debug.LogWarning($"AudioManager : Limits exceded for _audioSource, maybe you need to increase your audioSourcePoolSize (Size = {Instance.audioSourcePoolSize})");
                return ;
            }
            audioPlayer.gameObject.transform.position = position;
            audioPlayer.gameObject.SetActive(true);
            audioPlayer.Play(clip);
            if (clip.isPlaceholder)
            {
                Debug.LogWarning("[AudioManager] Placeholder sound was played, name " + aliaseName);
            }
            if (!String.IsNullOrEmpty(clip.Secondary))
            {
                PlaySoundAtPosition(clip.Secondary, position);
            }
            audioPlayerLoop = audioPlayer;
        }
        public static void StopLoopSound(AudioPlayer audioPlayer)
        {
            if (audioPlayer != null)
            {
                audioPlayer.StopLoopSound();
            }
        }
    }
    
    public class InvalidAliasesException : Exception {
        public InvalidAliasesException() : base() { }

        public InvalidAliasesException(string message) : base(message) { }

        public InvalidAliasesException(string message, Exception innerException) : base(message, innerException) { }
    }
}