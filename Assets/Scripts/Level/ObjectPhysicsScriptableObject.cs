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
    [Aliase] public string aliaseAmbiant;
    [Aliase] public string aliaseMoving;
    [Aliase] public string aliaseImpact;
    [Aliase] public string aliaseDeath;

    [Header("FX")] 
    public FXScriptableObject fxHit;
    public FXScriptableObject fxDeath;
    public FXScriptableObject fxMovement;

}
