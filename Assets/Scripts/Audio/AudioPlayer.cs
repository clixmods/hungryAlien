
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


namespace AudioAliase
{
    public enum CurrentlyPlaying
    {
        /// <summary>
        /// The start sound is defined
        /// </summary>
        Start,
        Base,
        End,
    }
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        public Queue<Aliase> _clips = new();
        [SerializeField] private Aliase _lastAliasePlayed;
        [SerializeField] private bool forceStop;
        
        #region Private Variable
        
        private bool _startWasPlayed;
        // TODO : Not good to use aliase in properties because it will be copied (serialize shit), we need to use simply string
        private Aliase _nextSound;
        private Transform _transformToFollow;
        private float _timePlayed = 0;
        private CurrentlyPlaying _state = CurrentlyPlaying.Start;
        #endregion
       

        public AudioSource Source { get; private set; }
        /// <summary>
        /// AudioPlayer is available ? 
        /// </summary>
        public bool IsUsable => _clips.Count == 0 && !Source.isPlaying && !gameObject.activeSelf;
        public bool IsFollowingTransform => _transformToFollow != null;
        
        public void SetTransformToFollow(Transform transformTarget)
        {
            _transformToFollow = transformTarget;
        }

        #region Event function
        
            private void Awake()
            {
                Source = transform.GetComponent<AudioSource>();
            }

            // Update is called once per frame
            private void Update()
            {
                if(_lastAliasePlayed == null)  gameObject.SetActive(false);
                //WatchToStopPlay();
                FollowTransform();

               
                if (_timePlayed >= Source.clip.length)
                {
                    if (_lastAliasePlayed.isLooping)
                    {
                        SetupAudioSource(_lastAliasePlayed);
                        _timePlayed = 0;
                    }
                    else // End of the sound
                    {
                        switch(_state)
                        {
                            case CurrentlyPlaying.Start:
                                //_state = CurrentlyPlaying.Base;
                                SetupAudioSource(_nextSound);
                                Source.clip = _nextSound.Audio; 
                                Source.Play();
                                break;
                            case CurrentlyPlaying.Base:
                                StopSound();
                                break;
                            case CurrentlyPlaying.End:
                            default:
                                gameObject.SetActive(false);
                                Reset();
                                break;
                        }

                        _timePlayed = 0;
                        _state++;
                    }
                }
                else
                {
                    _timePlayed += Time.deltaTime;
                }
    
            }
            private void Reset()
            {
                Source.Stop();
                _lastAliasePlayed = null;
                _transformToFollow = null;
                _state = CurrentlyPlaying.Start;
                _timePlayed = 0;
                _nextSound = null;
            }


        #endregion
        
        private void Play(Aliase aliaseToPlay)
        {
            
            // If a start aliase is available, we need to play it before the base aliase
            if (_state == CurrentlyPlaying.Start && AudioManager.GetSoundByAliase(aliaseToPlay.startAliase, out Aliase startLoop))
            {
                //_state = CurrentlyPlaying.Start;
                SetupAudioSource(startLoop);
                Source.clip = startLoop.Audio;
                Source.Play();
                _nextSound = aliaseToPlay;
                return;
            }
          
            _state = CurrentlyPlaying.Base; // Sinon ca fait le bug du next sound pas def
            
            //Setup the base aliase
            //_state = CurrentlyPlaying.Base;
            SetupAudioSource(aliaseToPlay);
            Source.clip = aliaseToPlay.Audio; 
            Source.Play();
        }

        public void Setup(Aliase aliaseToPlay)
        {
            Reset();
            Play(aliaseToPlay);
        }
        
      
      

        void FollowTransform()
        {
            if (IsFollowingTransform)
            {
                transform.position = _transformToFollow.position;
            } 
        }
        public void StopSound()
        {
            Source.Stop();
            if (_state == CurrentlyPlaying.Start)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (_state == CurrentlyPlaying.Base 
               // && _lastAliasePlayed != null
                && !string.IsNullOrEmpty(_lastAliasePlayed.endAliase)
                && AudioManager.GetSoundByAliase(_lastAliasePlayed.endAliase, out Aliase stopLoop) )
            {
               // _state = CurrentlyPlaying.End;
                SetupAudioSource(stopLoop);
                Source.clip = stopLoop.Audio;
                Source.Play();
            }
            else
            {
                gameObject.SetActive(false);
            }

            _state++;
        }

        public void SetupAudioSource(Aliase aliase)
        {
            if (aliase == null)
            {
                Debug.LogError("What the fuck ?");
            }
            _timePlayed = 0;
            _lastAliasePlayed = aliase;
            var audiosource = Source;
            audiosource.volume = Random.Range(aliase.minVolume, aliase.maxVolume);
            audiosource.loop = aliase.isLooping;
            audiosource.pitch = Random.Range(aliase.minPitch, aliase.maxPitch);

            audiosource.spatialBlend = aliase.spatialBlend;
            if (aliase.MixerGroup != null)
                audiosource.outputAudioMixerGroup = aliase.MixerGroup;

            switch (aliase.CurveType)
            {
                case AudioRolloffMode.Logarithmic:
                case AudioRolloffMode.Linear:
                    audiosource.rolloffMode = aliase.CurveType;
                    break;
                case AudioRolloffMode.Custom:
                    audiosource.rolloffMode = aliase.CurveType;
                    audiosource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, aliase.distanceCurve);
                    break;

            }
        }
    }
}