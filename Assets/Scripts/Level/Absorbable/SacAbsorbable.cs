using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Level;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;

namespace Level
{
    public class SacAbsorbable : MonoBehaviour, IAbsorbable
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

        [Level, SerializeField] int level;
        [SerializeField] GameObject _fourche;
        [SerializeField] GameObject _book;
        [SerializeField] LayerMask _layerMask;
        public bool _isBroken;
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
        }
        private void LateUpdate()
        {
            _isInAbsorbing = false;
        }

        public float HeightObject { get; }

        public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            
        }

        public bool OnTrigger(Absorber absorber)
        {
            return true;
        }

        public void WakeObject()
        {
            throw new System.NotImplementedException();
        }
    }
}