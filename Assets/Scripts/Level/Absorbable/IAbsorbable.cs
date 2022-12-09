using UnityEngine;

namespace Level
{
    
    public interface IAbsorbable
    {
        /// <summary>
        /// Ability to absorb the target or not
        /// </summary>
        public bool IsAbsorbable { get; }
        public bool IsAbsorbed { get; }
        public Rigidbody Rigidbody { get; }
  //      public bool IgnoreForceRequired { get; }
        /// <summary>
        /// Force required to absorb the object
        /// </summary>
        public float ForceRequired { get; }
        public Transform transform { get;  }
        public GameObject gameObject { get;  }
        public bool SleepUntilAbsorb { get; set; }
        /// <summary>
        /// PlayableVolume in contact with the GameObject
        /// </summary>
        public PlayableVolume PlayableVolume { get; set; }
        /// <summary>
        /// Initial position when the object is activated for the first time
        /// </summary>
        public Vector3 InitialPosition { get; set; }

        public float HeightObject { get; }
        /// <summary>
        /// Behaviour when the object is absorbing
        /// </summary>
        /// <param name="absorber"></param>
        /// <param name="absorbingState"></param>
        /// <returns> Absorb is completed?</returns>
        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState);
        public bool OnTrigger(Absorber absorber);
        public void WakeObject();
    }
}