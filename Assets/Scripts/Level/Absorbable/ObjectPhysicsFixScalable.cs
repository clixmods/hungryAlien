using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Level
{
    public class ObjectPhysicsFixScalable : ObjectPhysics
    {
        private float _absorbtionAmount = 0;
        [SerializeField] private float _absortionMultiplier = 0.2f;
        
        [SerializeField] private float absorbedScale = 0.5f;

        Vector3 targetScale
        {
            get
            {
                return new Vector3(absorbedScale, absorbedScale, absorbedScale);
            }
        }
        
        public override void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
        {
            IsInAbsorbing = true;
            absorbingState = AbsorbingState.InProgress;
                
            var destination = absorber.AbsorbePoint.position;
            float idkneedtobedefined = destination.y - transform.position.y;
            var direction = destination - transform.position;
            float forceRemaining = absorber.Strenght / ForceRequired;
            // We have the required Force
            if (forceRemaining >= 1)
            {
                _absorbtionAmount += Time.deltaTime * _absortionMultiplier;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, _absorbtionAmount);
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
            if (_absorbtionAmount >= 1)
            {
                absorber.Strenght += ScaleMultiplier;
                IsAbsorbed = true;
                EndObject();
                absorbingState = AbsorbingState.Done;
            }
        }

    }
}