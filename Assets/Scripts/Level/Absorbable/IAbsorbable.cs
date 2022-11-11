using UnityEngine;

namespace Level
{
    
    public interface IAbsorbable
    {
        public bool IsGrabable { get; }
        public bool IsAbsorbed { get; }
        public Rigidbody Rigidbody { get; }
        
        public float ForceRequired { get; }
        /// <summary>
        /// Behaviour when the object is absorbing
        /// </summary>
        /// <param name="absorber"></param>
        /// <returns> Absorb is completed?</returns>
        public bool OnAbsorb(Absorber absorber, out AbsorbingState absorbingState);
    }
}