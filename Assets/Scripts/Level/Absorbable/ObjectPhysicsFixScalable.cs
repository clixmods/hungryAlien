using System.Numerics;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Vector3 = UnityEngine.Vector3;

namespace Level
{
    public class ObjectPhysicsFixScalable : ObjectPhysics
    {
        private float _absorbtionAmount = 0;
        [SerializeField] private float _absortionMultiplier = 0.2f;
        
        [SerializeField] private float absorbedScale = 0.5f;

        private Vector3 _startScale;
        Vector3 targetScale
        {
            get
            {
                return new Vector3(absorbedScale, absorbedScale, absorbedScale);
            }
        }

        public override void WakeObject()
        {
            base.WakeObject();
            _startScale = transform.localScale;
        }

        protected override void OnAbsorbed()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero,  Time.deltaTime * _absortionMultiplier);
            if(transform.localScale.magnitude < 0.05f)
                Destroy(gameObject);
        }

        protected override void OnIsAbsorbing()
        {
            //base.OnIsAbsorbing();
            transform.localScale = Vector3.Lerp(_startScale, targetScale, _absorbtionAmount);
        }

        protected override void OnStopAbsorbing()
        {
            //base.OnStopAbsorbing();
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
            if (_absorbtionAmount >= absorbedScale)
            {
                absorber.Strenght += ScaleMultiplier;
                IsAbsorbed = true;
                EndObject();
                absorbingState = AbsorbingState.Done;
            }
        }

    }
}