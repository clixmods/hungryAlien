using System;
using UnityEngine;
using UnityEngine.VFX;

namespace FX
{
    public class FXInstance : MonoBehaviour
    {
        private FXScriptableObject fxAsset;
        private VisualEffect _visualEffect;
        private ParticleSystem _particleSystem;
            
        private void Start()
        {
            //throw new NotImplementedException();
            switch (fxAsset.TypeFX)
            {
                case TypeFX.VisualEffect:
                    _visualEffect = GetComponent<VisualEffect>();
                    break;
                case TypeFX.ParticleSystem:
                    _particleSystem = GetComponent<ParticleSystem>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Update()
        {
            // switch (fxAsset.TypeFX)
            // {
            //     case TypeFX.VisualEffect:
            //         if(_visualEffect.visualEffectAsset.)
            //         break;
            //     case TypeFX.ParticleSystem:
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
        }
        
        // public sealed override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        // {
        //     if (state.playing)
        //     {
        //         float currentRate = vfxValues.GetFloat(rateID);
        //         state.spawnCount += currentRate * state.deltaTime;
        //     }
        // }
    }
}