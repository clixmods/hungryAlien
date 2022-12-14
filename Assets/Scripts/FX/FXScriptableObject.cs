using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public enum TypeFX
{
    VisualEffect,
    ParticleSystem
}

[CreateAssetMenu(fileName = "FX_", menuName = "FX/Fx Asset", order = 1)]
public class FXScriptableObject : ScriptableObject
{ 
    public GameObject _fxPrefab;
  
   // public VisualEffect _visualEffect;
    public bool isPlaceholder = true;
    private TypeFX _typeFX;
    public TypeFX TypeFX { get; private set; }
    private void OnValidate()
    {
        GetType();
    }

    private void Awake()
    {
        GetType();
    }

    void GetType()
    {
        if (_fxPrefab.TryGetComponent<VisualEffect>( out VisualEffect ve))
        {
            TypeFX = TypeFX.VisualEffect;
        }

        if (_fxPrefab.TryGetComponent<ParticleSystem>( out ParticleSystem particleSystem))
        {
            TypeFX = TypeFX.ParticleSystem;
        }
    }

    public bool Spawn(Vector3 position, Transform transformParent, out GameObject fxObject)
    {
        fxObject = Instantiate(_fxPrefab,position,Quaternion.identity,transformParent);
        return true;
    }
    //public GameObject _visualEffect;
}

