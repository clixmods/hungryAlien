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
        public Rigidbody Rigidbody { get; private set; }
        public bool IgnoreForceRequired { get; }
        public float ForceRequired { get; }
        public bool SleepUntilAbsorb { get; set; }
        public PlayableVolume PlayableVolume { get; set; }
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
        }

        
        public Vector3 InitialPosition { get; set; }

        public float HeightObject { get; }

        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - transform.position;
          
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