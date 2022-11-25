using System.Collections;
using System.Collections.Generic;
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
        public Rigidbody Rigidbody { get; private set; }
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
        private GameObject water;
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
            currentHeight = 0f;
            LevelManager.Instance.CallbackLevelChange += DeactivateRocks;
        }
        private void LateUpdate()
        {
            _isInAbsorbing = false;
        }
        private void Update()
        {
            if (_isInAbsorbing)
            {
                if(water != null)
                {
                    water.transform.position = new Vector3(ShipController._instance.transform.position.x, transform.position.y, ShipController._instance.transform.position.z);
                    if(!water.GetComponent<ParticleSystem>().isPlaying)
                    {
                        water.GetComponent<ParticleSystem>().Play();
                        water.GetComponent<ParticleSystem>().Play();
                    }
                }
                
            }
            else
            {
                if (water != null)
                {
                    water.GetComponent<ParticleSystem>().Stop();
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
        public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            _isInAbsorbing = true;

            if (currentHeight < _heightToActivateRocks)
            {
                var direction = Vector3.down;
                transform.position -= direction * _speed * Time.deltaTime;
                currentHeight -= _speed * Time.deltaTime;
                if (water == null)
                {
                    water = Instantiate(_VFXWater);
                }
            }
            else
            {
                absorbingState = AbsorbingState.Done;
                for(int i = 0; i < _rocks.Count; i++)
                {
                    _rocks[i].IsAbsorbable = true;
                }
                Destroy(water);
            }
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
