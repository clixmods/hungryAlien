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
public class ObjectPhysics : MonoBehaviour , IAbsorbable
{
    private const string MessageSettingsNotSetup = "Settings of object are not setup, please assign a setting.";
    private const float _regenMultiplier = 0.2f;

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
    [Range(0,1f),SerializeField] private float scaleMultiplier = 0.05f ;
    

    #endregion
    #region Private Variable
    /// <summary>
    /// AudioPlayer used for loop sound etc, cached to be stopped when its desired
    /// </summary>
    private AudioPlayer _audioPlayer;
    private MeshCollider _collider;
    private PlayableVolume _playableVolume;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock[] _propBlocks;
    #endregion
    #region Properties
    public Rigidbody Rigidbody { get; private set; }
    public float ForceRequired => forceRequired;
    public bool IsAbsorbed { get; set; }
    public bool IsAbsorbable { get; private set; }
    
    public bool IsInAbsorbing { get; set; }
    
    public Vector3 InitialPosition { get;  set; }
    
    public int SleepUntilLevel => sleepUntilLevel;
    public float ScaleMultiplier => scaleMultiplier;

    [SerializeField] private bool _sleepUntilAbsorb;

    public bool SleepUntilAbsorb
    {
        get { return _sleepUntilAbsorb; }
        set { _sleepUntilAbsorb = value; }
    }

    public PlayableVolume PlayableVolume { get; set; }
    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _propBlocks = new MaterialPropertyBlock[_meshRenderer.materials.Length];
        for (int i = 0; i < _propBlocks.Length; i++)
        {
            _propBlocks[i] = new MaterialPropertyBlock();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitialPosition = transform.position;
        
        _collider = GetComponent<MeshCollider>();
        if (_collider == null)
            _collider = transform.AddComponent<MeshCollider>();
        
        _collider.convex = true;
        _collider.enabled = false;
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.isKinematic = true;
        Rigidbody.mass = ForceRequired;
        
        
        if (settings == null)
        {
            Debug.LogWarning(MessageSettingsNotSetup, gameObject);
            // Prevent null ref
            settings = ObjectPhysicsScriptableObject.CreateInstance<ObjectPhysicsScriptableObject>(); 
           
        }

        LevelManager.Instance.CallbackPreLevelChange += WatchLevelToWakeUp;

        if (ForceRequired == 0)
        {
            Debug.LogWarning("Warning : Force required = 0, assign a greater value.", gameObject);
            forceRequired = 1;
        }
           
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
//            FXManager.PlayFXAtPosition(settings.fxHit, transform.position);
        }
    }

    private void Update()
    {
        if (PlayableVolume == null)
        {
            
            transform.position = InitialPosition;
            Rigidbody.Sleep();
            
        }
        else
        {
            Rigidbody.WakeUp();
        }
        
    
            
    }

    private void OnDestroy()
    {
        
      
    }

    void EndObject()
    {
        AudioManager.PlaySoundAtPosition(settings.aliaseDeath, transform.position);
        //FXManager.PlayFXAtPosition(settings.fxDeath,transform.position);
        AudioManager.StopLoopSound(ref _audioPlayer);
        LevelManager.Instance.RemoveObjectPhysical(this);
    }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Gizmos.DrawWireSphere(AbsorbePoint.position, 1);
            Handles.Label(transform.position, 
                $"Force Required {ForceRequired} // Gain : {scaleMultiplier}",
                new GUIStyle());
        }
    #endif

    #endregion
    
    // TODO : We need to let the level manager manages that
    void WatchLevelToWakeUp()
    {
        if ( !IsAbsorbable && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
        {
            _collider.enabled = true;
           
            LevelManager.Instance.AddObjectPhysical(this);
            LevelManager.Instance.CallbackPreLevelChange -= WatchLevelToWakeUp;
            IsAbsorbable = true;
            Rigidbody.isKinematic = SleepUntilAbsorb;
           
        }
    }


    void SetDissolve(float amount)
    {
        for (int i = 0; i < _propBlocks.Length ; i++)
        {
            // Get the current value of the material properties in the renderer.
            _meshRenderer.GetPropertyBlock(_propBlocks[i]);
            // Assign our new value.
            _propBlocks[i].SetFloat("_Amount", amount);
            // Apply the edited values to the renderer.
            _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
        }
    }

    private void FixedUpdate()
    {
        if (!IsInAbsorbing)
        {
            for (int i = 0; i < _propBlocks.Length ; i++)
            {
                // Get the current value of the material properties in the renderer.
                _meshRenderer.GetPropertyBlock(_propBlocks[i],i);
                 //Assign our new value.
                _propBlocks[i].SetFloat("_Amount",  _propBlocks[i].GetFloat("_Amount")-Time.deltaTime*_regenMultiplier);
                // Apply the edited values to the renderer.
                _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
            }
        }
        else
        {
            for (int i = 0; i < _propBlocks.Length ; i++)
            {
                // Get the current value of the material properties in the renderer.
                _meshRenderer.GetPropertyBlock(_propBlocks[i],i);
                //Assign our new value.
                _propBlocks[i].SetFloat("_Amount",  _propBlocks[i].GetFloat("_Amount")+Time.deltaTime);
                // Apply the edited values to the renderer.
                _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
            }
        }

        if (IsAbsorbed)
        {
            bool destroyIt = true;
            for (int i = 0; i < _propBlocks.Length ; i++)
            {
                _meshRenderer.GetPropertyBlock(_propBlocks[i],i);
                if (_propBlocks[i].GetFloat("_Amount") < 1)
                    destroyIt = false;
                _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
            }
            if(destroyIt)
                Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if(!IsAbsorbed)
            IsInAbsorbing = false;
    }

    public void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
    {
        IsInAbsorbing = true;
        absorbingState = AbsorbingState.InProgress;
       
        Rigidbody.isKinematic = false;
        
        
        float forceRemaining = absorber.Strenght / ForceRequired;
        var destination = absorber.AbsorbePoint.position;
        var direction = destination - transform.position;
        direction *= (forceRemaining);
        Rigidbody.velocity = direction;

        bool forceIsSufficent = forceRemaining >= 1;

        float idkneedtobedefined = destination.y - transform.position.y;
         if (idkneedtobedefined < 5)
         {
             SetDissolve(1-(idkneedtobedefined/5));
         }
        
        
        if (forceRemaining < 1)
        {
           // absorber.Ship.enabled = false;
            absorber.Ship.transform.position += -direction * forceRemaining * Time.deltaTime;
        }
        // Ship can absorb
        if (forceIsSufficent && idkneedtobedefined < absorber.AbsortionHeight)
        {
            absorber.Strenght += ScaleMultiplier;
            IsAbsorbed = true;
            EndObject();
            absorbingState = AbsorbingState.Done;
          
       
        }
        else if (!forceIsSufficent && idkneedtobedefined < absorber.AbsortionHeight)
        {
            absorbingState = AbsorbingState.Fail;
            //SetDissolve(0);
        }

   

    }

    public void ResetToInitialPosition()
    {
        throw new NotImplementedException();
    }
}
