using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AudioAliase;

[CreateAssetMenu(fileName = "objphysics_", menuName = "Gameplay Element/Object Physics", order = 1)]
public class ObjectPhysicsScriptableObject : ScriptableObject
{
    public float hellowork;
    
    [SerializeField] public int sleepUntilLevel;
    
    [Range(0,10)]
    [SerializeField] public float magnitudeToStopLoop = 0;

    [Range(1,2)]
    [SerializeField] public float scaleMultiplier = 1.05f ;

    [Header("Sound Aliases")]
    [Aliase] public string aliaseAmbiant;
    [Aliase] public string aliaseMoving;
    [Aliase] public string aliaseDeath;
}
