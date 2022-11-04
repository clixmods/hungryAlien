using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
using FX;
using Unity.VisualScripting;
using UnityEditor;
using Level;



[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour
{
    private const string MessageSettingsNotSetup = "Settings of object are not setup, please assign a setting.";

    #region SerializeField

    /// <summary>
    /// Settings of the object, contains sound, FX and some arts data info
    /// </summary>
    [SerializeField] private ObjectPhysicsScriptableObject settings;
    /// <summary>
    /// Indicate level related to this object.
    /// </summary>
    [SerializeField,Level] private int sleepUntilLevel;
    /// <summary>
    /// Force required to absorb the object by the player
    /// </summary>
    [SerializeField,Range(0.01f, 2)] private float forceRequired;
    /// <summary>
    /// The gain of the absorbtion 
    /// </summary>
    [Range(1,2),SerializeField] private float scaleMultiplier = 1.05f ;
    

    #endregion
    #region Private Variable
    /// <summary>
    /// AudioPlayer used for loop sound etc, cached to be stopped when its desired
    /// </summary>
    private AudioPlayer _audioPlayer;
    private MeshCollider _collider;
    #endregion
    #region Properties
    public Rigidbody ObjectRigidbody { get; private set; }
    public int SleepUntilLevel => sleepUntilLevel;
    public float ScaleMultiplier => scaleMultiplier;
    public float ForceRequired => forceRequired;
    public bool IsAbsorbed { get; set; }
    public bool IsGrabable { get; private set; }
    #endregion

    #region MonoBehaviour

    // Start is called before the first frame update
    void Start()
    {
        
        _collider = GetComponent<MeshCollider>();
        if (_collider == null)
            _collider = transform.AddComponent<MeshCollider>();
        
        _collider.convex = true;
        ObjectRigidbody = GetComponent<Rigidbody>();
        ObjectRigidbody.isKinematic = true;
        ObjectRigidbody.mass = ForceRequired;
        
        if (settings == null)
        {
            Debug.LogWarning(MessageSettingsNotSetup, gameObject);
            // Prevent null ref
            settings = ObjectPhysicsScriptableObject.CreateInstance<ObjectPhysicsScriptableObject>(); 
            //gameObject.SetActive(false);
        }

        LevelManager.Instance.CallbackPreLevelChange += WatchLevelToWakeUp;

    }

    // Update is called once per frame
    void Update()
    {
        ObjectRigidbody.mass = ForceRequired; // TODO : TEMP
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            if (collision.gameObject.TryGetComponent<CollisionSurface>(out var collisionSurface))
            {
                collisionSurface.Play();
            }
            AudioManager.PlaySoundAtPosition(settings.aliaseImpact,transform.position);
        }
    }
    
    private void OnDestroy()
    {
        
        AudioManager.PlaySoundAtPosition(settings.aliaseDeath, transform.position);
        //FXManager.PlayFXAtPosition(settings.fxDeath,transform.position);
        AudioManager.StopLoopSound(ref _audioPlayer);
        LevelManager.Instance.RemoveObjectPhysical(this);
    }
    
    private void OnDrawGizmos()
    {
        // Gizmos.DrawWireSphere(AbsorbePoint.position, 1);
        Handles.Label(transform.position, 
            $"Force Required {ForceRequired} // Gain : {scaleMultiplier}",
            new GUIStyle());
    }

    #endregion
    
    // TODO : We need to let the level manager manages that
    void WatchLevelToWakeUp()
    {
        if ( ObjectRigidbody.isKinematic && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
        {
            Debug.Log("[ObjectPhysics] Object added to LevelManager", gameObject);
            ObjectRigidbody.isKinematic = false;
            LevelManager.Instance.AddObjectPhysical(this);

            int test = LevelManager.Instance.CallbackPreLevelChange.GetInvocationList().Length;
            LevelManager.Instance.CallbackPreLevelChange -= WatchLevelToWakeUp;
            Debug.LogWarning($"OH donc {test} est devenue { LevelManager.Instance.CallbackPreLevelChange.GetInvocationList().Length}");
            IsGrabable = true;
        }
    }
}
