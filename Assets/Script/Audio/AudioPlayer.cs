using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AudioAliase
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {
        AudioSource _audioSource;
        public Queue<Aliase> _clips = new();


        public AudioSource Source
        {
            get { return _audioSource; }
        }

        public bool IsUsable
        {
            get
            {
                return _clips.Count == 0 && !Source.isPlaying;
            }
        }
        // Start is called before the first frame update
        private void Awake()
        {
            _audioSource = transform.GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Debug.Log("AudioPlayer : Current Clips in queue :" + _clips.Count);
            if (!_audioSource.isPlaying && _clips.Count > 0)
            {
                Aliase newAliaseToPlay = _clips.Dequeue();
                SetupAudioSource(newAliaseToPlay);
                
                if (newAliaseToPlay.isLooping)
                {
                    Source.clip = newAliaseToPlay.Audio;
                    Source.Play();
                }
                else
                {
                    Source.PlayOneShot(newAliaseToPlay.Audio, newAliaseToPlay.volume);
                }
            }

            if (!_audioSource.isPlaying && _clips.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetupAudioSource(Aliase aliase)
        {
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
    }
}