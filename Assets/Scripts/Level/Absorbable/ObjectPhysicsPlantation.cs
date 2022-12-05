using UnityEngine;

namespace Level
{
    public class ObjectPhysicsPlantation : ObjectPhysics
    {
       [SerializeField] private float minHeightToActiveGravity = 5;
       private bool _isImplanted; 
       protected override void WakeObject()
       {
           base.WakeObject();
           Rigidbody.isKinematic = true;
           Rigidbody.useGravity = false;
       }
       
       
       public override void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
       {
           if (_isImplanted)
           {
               base.OnAbsorb(absorber, out absorbingState);
               
               return;
           }
           Rigidbody.isKinematic = false;
           float heightToActiveGravity = minHeightToActiveGravity + InitialPosition.y;
           var position = transform.position;
           position.y += Time.deltaTime;
           transform.position = position;
           if (!_isImplanted && transform.position.y >= heightToActiveGravity)
           {
               Rigidbody.isKinematic = false;
               Rigidbody.useGravity = true;
               _isImplanted = true;
           }

           absorbingState = AbsorbingState.InProgress;

       }

       protected override void LateUpdate()
       {
           base.LateUpdate();
           if (!_isImplanted)
           {
               Rigidbody.isKinematic = true;
           }
           
       }
       
    }
}