using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Level;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;

namespace Level
{
    public class RateauAbsorbable : MonoBehaviour, IAbsorbable
    {
        public bool IsAbsorbable { get; private set; }
        public bool IsAbsorbed { get; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IgnoreForceRequired { get; }
        public float ForceRequired { get; }
        public bool SleepUntilAbsorb { get; set; }
        public PlayableVolume PlayableVolume { get; set; }
        public Vector3 InitialPosition { get; set; }
        private bool _isInAbsorbing = false;

        [Level, SerializeField] int level;
        private Animator _animator;
        [SerializeField] GameObject _wood;
        [SerializeField] GameObject _halfWood1, _halfWood2;
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
            _animator = GetComponent<Animator>();
        }
        private void LateUpdate()
        {
            _isInAbsorbing = false;
        }

        public float HeightObject { get; }

        public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            _animator.SetTrigger("Rateau");
            _wood.SetActive(false);
            _halfWood1.SetActive(true);
            _halfWood2.SetActive(true);
            
        }

        public bool OnTrigger(Absorber absorber)
        {
            throw new System.NotImplementedException();
        }
    }
}