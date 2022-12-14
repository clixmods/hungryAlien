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
        [SerializeField] private float _speedAbsorbsion = 1;
        private void Start()
        {
            InitialPosition = transform.position;
            Rigidbody = GetComponent<Rigidbody>();
            IsAbsorbable = true;
            Rigidbody.isKinematic = SleepUntilAbsorb;
            
        }

        private void Update()
        {
            if ( PlayableVolume == null)
            {
                transform.position = InitialPosition;
                Rigidbody.Sleep();
            }
            else
            {
                if(Rigidbody.IsSleeping() && !Rigidbody.isKinematic)
                    Rigidbody.WakeUp();
            }
            
            if (!IsInAbsorbing && Rigidbody.velocity.y > 0)
            {
                
                Rigidbody.velocity = new Vector3(Rigidbody.velocity.x,0,Rigidbody.velocity.z);
            }
        }


        public Vector3 InitialPosition { get; set; }

        public float HeightObject { get; }

        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            if(SleepUntilAbsorb)
                Rigidbody.isKinematic = false;

            IsInAbsorbing = true;
            absorbingState = AbsorbingState.InProgress;
            var destination = absorber.AbsorbePoint.position - new Vector3(0,_maxHeightAbsorbtion,0);
            var direction = destination - (transform.position );
          
            Rigidbody.velocity = direction * _speedAbsorbsion;

       
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