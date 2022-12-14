using System;
using UnityEngine;

namespace FX
{
    public class FXManager : MonoBehaviour
    {

        
        private static FXManager _instance;
        private static FXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<FXManager>();
                    if(_instance == null)
                        _instance = new GameObject("FXManager").AddComponent<FXManager>();
                }

                return _instance;
            }
            set => _instance = value;
        }

        private void Awake()
        {
            Instance = this;
        }

        public static void PlayFXAtPosition(FXScriptableObject fxAsset, Vector3 position, Transform parent =null) 
        {
            if (fxAsset == null) return;
            if (fxAsset.Spawn(position, Instance.transform, out var fxObject))
            {
                var fxComponent = fxObject.AddComponent<FXInstance>();
                fxComponent.SetFXAsset(fxAsset);
         
            }
               
        }
    }
}