using AudioAliase;
using UnityEngine;

namespace Level
{
    public class ObjectPhysicsPlantation : ObjectPhysics
    {
       [SerializeField] private float minHeightToActiveGravity = 5;
       private const float SpeedDeplantation = 3;
       private bool _isImplanted;

       [SerializeField] [Aliase] private string surfaceDebrisAliasSound = "debris_dirt";
       public override void WakeObject()
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
           position.y += Time.deltaTime * SpeedDeplantation;
           transform.position = position;
           if (!_isImplanted && transform.position.y >= heightToActiveGravity)
           {
               AudioManager.PlaySoundAtPosition(surfaceDebrisAliasSound, transform.position); 
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