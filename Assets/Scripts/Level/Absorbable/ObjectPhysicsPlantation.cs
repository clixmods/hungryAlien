using UnityEngine;

namespace Level
{
    public class ObjectPhysicsPlantation : ObjectPhysics
    {
       [SerializeField] private float minHeightToActiveGravity = 5;

       protected override void WakeObject()
       {
           base.WakeObject();
           Rigidbody.useGravity = false;
       }
       public override void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
       {
           base.OnAbsorb(absorber, out absorbingState);
           float heightToActiveGravity = minHeightToActiveGravity + InitialPosition.y;
           if (transform.position.y >= heightToActiveGravity)
           {
               Rigidbody.isKinematic = false;
               Rigidbody.useGravity = true;
           }
       }
    }
}