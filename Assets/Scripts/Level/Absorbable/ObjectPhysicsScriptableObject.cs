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
    [Aliase] public string OnAmbiantAliaseSound;
    [Aliase] public string OnMovingAliaseSound;
    [Aliase] public string OnImpactAliaseSound;
    [Aliase] public string OnDeathAliaseSound;

    [Header("FX")] 
    public FXScriptableObject OnHitFX;
    public FXScriptableObject OnDeathFX;
    public FXScriptableObject OnMovementFX;

}
