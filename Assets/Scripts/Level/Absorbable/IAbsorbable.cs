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
        /// <summary>
        /// Force required to absorb the object
        /// </summary>
        public float ForceRequired { get; }

        public Transform transform { get;  }
        
        public Vector3 InitialPosition { get; set; }
        /// <summary>
        /// Behaviour when the object is absorbing
        /// </summary>
        /// <param name="absorber"></param>
        /// <param name="absorbingState"></param>
        /// <returns> Absorb is completed?</returns>
        public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState);
        

    }
}