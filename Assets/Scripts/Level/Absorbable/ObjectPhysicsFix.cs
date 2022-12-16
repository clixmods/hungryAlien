using UnityEngine;

namespace Level
{
    public class ObjectPhysicsFix : ObjectPhysics
    {
        private float _absorbtionAmount = 0;
        [SerializeField]
        [Range(0.1f,1f)]
        private float thresholdToBeAsborbed = 0.7f;
        [SerializeField] private float _absortionMultiplier = 0.2f;
        
        public override void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            IsInAbsorbing = true;
            absorbingState = AbsorbingState.InProgress;
            Rigidbody.isKinematic = true;
                
            var destination = absorber.AbsorbePoint.position;
            float idkneedtobedefined = destination.y - transform.position.y;
            var direction = destination - transform.position;
            float forceRemaining = absorber.Strenght / ForceRequired;
            // We have the required Force
            if (forceRemaining >= 1)
            {
                _onAbsorbFX.PlayFXAtPosition(true,transform.position);
                _absorbtionAmount += Time.deltaTime * _absortionMultiplier;
                SetDissolve(_absorbtionAmount);
            }
            else // Do attraction to object
            {
                if(idkneedtobedefined < absorber.AbsortionHeight)
                {
                    absorbingState = AbsorbingState.Fail;
                    return;
                }
                
                absorber.Ship.transform.position += -direction * forceRemaining * Time.deltaTime;
            }
            // Object is absorbed ?
            if (_absorbtionAmount >= thresholdToBeAsborbed)
            {
                absorber.Strenght += ScaleMultiplier;
                IsAbsorbed = true;
                EndObject();
                absorbingState = AbsorbingState.Done;
            }
        }

    }
}