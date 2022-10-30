using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioAliase
{
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
            void Update()
            {
                WatchToStopPlay();
                FollowTransform();
            }
            private void OnDisable()
            {
                _transformToFollow = null;
            }


        #endregion
        
        public void Play(Aliase aliaseToPlay)
        {
            // If a start aliase is available, we need to play it before the main aliase
            if (!_startWasPlayed && AudioManager.GetSoundByAliase(aliaseToPlay.startAliase, out Aliase startLoop))
            {
                SetupAudioSource(startLoop);
                Source.clip = startLoop.Audio;
                Source.Play();
                _startWasPlayed = true;
                _nextSound = aliaseToPlay;
                return;
            }
            
            //Setup the main aliase
            SetupAudioSource(aliaseToPlay);
            Source.clip = aliaseToPlay.Audio; 
            Source.Play();
        }
        
        void WatchToStopPlay()
        {
            // We check if the AudioPlayer is stopped
            if (forceStop || !Source.isPlaying )
            {
                forceStop = false;
                // If the start aliase was played, we need to play the owner of the startAliase
                if (_startWasPlayed)
                {
                    Play(_nextSound);
                    _startWasPlayed = false;
                    return;
                }
                
                // 
                if (_lastAliasePlayed != null && _lastAliasePlayed.isLooping)
                {
                    StopLoopSound();
                    return;
                }
                
                gameObject.SetActive(false);
            }
        }

      

        void FollowTransform()
        {
            if (IsFollowingTransform)
            {
                transform.position = _transformToFollow.position;
            } 
        }
        public void StopLoopSound()
        {
            Source.Stop();
            // If the stop happen when we play the start aliase, we stop it with a _startwasplayed false
            if (_startWasPlayed)
            {
                _startWasPlayed = false;
            }
            if (!_startWasPlayed && _lastAliasePlayed != null && AudioManager.GetSoundByAliase(_lastAliasePlayed.endAliase, out Aliase stopLoop))
            {
                SetupAudioSource( stopLoop);
                Source.clip = stopLoop.Audio;
                Source.Play();
            }
        }

        public void SetupAudioSource(Aliase aliase)
        {
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