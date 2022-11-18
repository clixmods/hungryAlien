using System;
using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshCollider))]
    public class ObjectPhysicsNotAbsorbable : MonoBehaviour , IAbsorbable
    {
        public bool IsAbsorbable { get; private set;}
        public bool IsAbsorbed { get; }
        public Rigidbody Rigidbody { get; private set; }
        public float ForceRequired { get; }
        public PlayableVolume PlayableVolume { get; set; }

        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
        }

        
        public Vector3 InitialPosition { get; set; }

        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - transform.position;
          
            Rigidbody.velocity = direction;

       
        }

    }
}