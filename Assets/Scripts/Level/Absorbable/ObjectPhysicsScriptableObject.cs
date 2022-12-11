using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AudioAliase;



[CreateAssetMenu(fileName = "objphysics_", menuName = "Gameplay Element/Object Physics", order = 1)]
public class ObjectPhysicsScriptableObject : ScriptableObject
{
   
    [Range(0,10)]
    [SerializeField] public float magnitudeToStopLoop = 0;

    
    [Header("Sound Aliases")]
    [Aliase] public string OnAmbiantAliaseSound = AudioManager.AliasNameNull;
    [Aliase] public string OnMovingAliaseSound = AudioManager.AliasNameNull;
    [Aliase] public string OnImpactAliaseSound = AudioManager.AliasNameNull;
    [Aliase] public string OnDeathAliaseSound = AudioManager.AliasNameNull;

    [Header("FX")] 
    public FXScriptableObject OnHitFX;
    public FXScriptableObject OnDeathFX;
    public FXScriptableObject OnAbsorbFX;
    public FXScriptableObject OnCancelAbsorbFX;
    //public FXScriptableObject OnMovementFX;

    private void OnValidate()
    {
        
    }

   
}
