using UnityEngine;

namespace Level
{
    public class AbsorbableTondeuse : MonoBehaviour, IAbsorbable
    {
        [SerializeField] private GameObject demarreur;
        
        
        
        public bool IsAbsorbable { get; }
        public bool IsAbsorbed { get; }
        public bool IsInAbsorbing { get; set; }

        public Rigidbody Rigidbody
        {
            get
            {
                return demarreur.GetComponent<Rigidbody>();
            }
        }

        public float ForceRequired { get; }
        public bool SleepUntilAbsorb { get; set; }
        public PlayableVolume PlayableVolume { get; set; }
        public Vector3 InitialPosition { get; set; }
        public float HeightObject { get; }
        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - transform.position;
            direction *= 3;
            
            Rigidbody.velocity = direction;
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