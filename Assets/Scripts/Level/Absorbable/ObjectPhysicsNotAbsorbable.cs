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

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
        }

        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            absorbingState = AbsorbingState.InProgress;
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - transform.position;
          
            Rigidbody.velocity = direction;

       
        }
    }
}