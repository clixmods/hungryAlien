using System.Collections;
using System.Collections.Generic;
using AudioAliase;
using UnityEngine;
using Level;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;

namespace Level
{
    public class WaterAbsorbable : MonoBehaviour, IAbsorbable
    {
        public bool IsAbsorbable { get; private set; }
        public bool IsAbsorbed { get; }
        public bool IsInAbsorbing { get; set; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IgnoreForceRequired { get; }
        public float ForceRequired { get; }
        public bool SleepUntilAbsorb { get; set; }
        public PlayableVolume PlayableVolume { get; set; }
        public Vector3 InitialPosition { get; set; }
        private bool _isInAbsorbing = false;

        [SerializeField] float _speed;
        [SerializeField] float _heightToActivateRocks;
        [Level, SerializeField] int level;
        private float currentHeight;
        [SerializeField] List<ObjectPhysics> _rocks;
        [SerializeField] GameObject _VFXWater;
        private ParticleSystem _particleSystemWater;
        // FX Cached
        protected ParticleSystem[] _onAbsorbFX;

        [SerializeField] [Aliase] private string OnAmbiantAliasSound;
        private AudioPlayer audioPlayerAmbiant;

        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
            currentHeight = 0f;
            LevelManager.Instance.CallbackLevelChange += DeactivateRocks;
            _onAbsorbFX = Instantiate(_VFXWater).GetComponentsInChildren<ParticleSystem>();
            AudioManager.PlayLoopSound(OnAmbiantAliasSound, transform.position, ref audioPlayerAmbiant);
            //_particleSystemWater = water.GetComponent<ParticleSystem>();
        }
        private void LateUpdate()
        {
            _isInAbsorbing = false;
        }
        private void Update()
        {
            
            if (_isInAbsorbing)
            {
                //if(water != null)
                {
                    var shipTransform = LevelManager.Instance.Player;
                    var position =  new Vector3(shipTransform.transform.position.x, transform.position.y, shipTransform.transform.position.z);
                    _onAbsorbFX.PlayFXAtPosition(true,position);
                    //if(!_particleSystemWater.isPlaying)
                    //{
                     //   _particleSystemWater.Play();
                     //   _particleSystemWater.Play();
                   // }
                }
                
            }
            else
            {
                //if (water != null)
                {
                    _onAbsorbFX.StopFX(false);
                    //_particleSystemWater.Stop();
                }
            }
        }
        private void ChangeMaterialsRenderQueue(int value)
        {
            var materials = GetComponent<MeshRenderer>().materials;
            foreach (var mtl in materials)
            {
                mtl.renderQueue = value;
            }
            GetComponent<MeshRenderer>().materials = materials;
        }

        public float HeightObject { get; }

        public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            _isInAbsorbing = true;

            if (currentHeight < _heightToActivateRocks)
            {
                var direction = Vector3.down;
                transform.position -= direction * _speed * Time.deltaTime;
                currentHeight -= _speed * Time.deltaTime;
              
            }
            else
            {
                absorbingState = AbsorbingState.Done;
                for(int i = 0; i < _rocks.Count; i++)
                {
                    _rocks[i].IsAbsorbable = true;
                }
                AudioManager.StopLoopSound(ref audioPlayerAmbiant);

                foreach (var ps in _onAbsorbFX)
                {
                    Destroy(ps.gameObject);
                }
               
            }
        }

        public bool OnTrigger(Absorber absorber)
        {
            return true;
        }

        public void WakeObject()
        {
            throw new System.NotImplementedException();
        }

        private void DeactivateRocks()
        {
            if(LevelManager.Instance.CurrentLevel == level)
            for (int i = 0; i < _rocks.Count; i++)
            {
                _rocks[i].IsAbsorbable = false;
            }
        }

    }
}
