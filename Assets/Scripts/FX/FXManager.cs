using System;
using UnityEngine;

namespace FX
{
    public class FXManager : MonoBehaviour
    {
        private static FXManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        public static void PlayFXAtPosition(FXScriptableObject fxAsset, Vector3 position)
        {
            GameObject fxInstance = fxAsset.Spawn(position, Instance.transform);
        }
    }
}