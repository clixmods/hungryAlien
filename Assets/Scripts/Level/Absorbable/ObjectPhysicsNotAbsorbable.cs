using System;
using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class ObjectPhysicsNotAbsorbable : MonoBehaviour , IAbsorbable
    {
        public bool IsAbsorbable { get; private set;}
        public bool IsAbsorbed { get; }
        public bool IsInAbsorbing { get; set; }
        public Rigidbody Rigidbody { get; private set; }
        public bool IgnoreForceRequired { get; }
        public float ForceRequired { get; }
        
        public bool SleepUntilAbsorb { get => _sleepUntilAsborb; set => _sleepUntilAsborb = value; }
        public PlayableVolume PlayableVolume { get; set; }

        [SerializeField] private bool _sleepUntilAsborb;
        [SerializeField] private float _maxHeightAbsorbtion = 5;
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
            Rigidbody.isKinematic = SleepUntilAbsorb;
            
        }

        
        public Vector3 InitialPosition { get; set; }

        public float HeightObject { get; }

        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            if(SleepUntilAbsorb)
                Rigidbody.isKinematic = false;
            
            absorbingState = AbsorbingState.InProgress;
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - (transform.position + new Vector3(0,_maxHeightAbsorbtion,0));
          
            Rigidbody.velocity = direction;

       
        }

        public bool OnTrigger(Absorber absorber)
        {
            return true;
        }

        public void WakeObject()
        {
            throw new NotImplementedException();
        }
    }
}