using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
using FX;
using Unity.VisualScripting;
using UnityEditor;
using Level;
using Unity.Mathematics;


public static class ExtensionFX
{
    public static void PlayFXAtPosition(this ParticleSystem[] particleSystems, bool isLooping, Vector3 position)
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            return;
        }
        foreach (var particleSystem in particleSystems)
        {
            particleSystem.transform.position = position;
            if (!particleSystem.isPlaying)
            {
                particleSystem.Play();
                if (isLooping)
                {
                    SetLoopBool(particleSystem, true);
                }
                    
            }
               
        }
    }

    public static void StopFX(this ParticleSystem[] particleSystems, bool isLooping)
    {
        if (particleSystems == null || particleSystems.Length == 0)
        {
            return;
        }

        foreach (var particleSystem in particleSystems)
        {
            if (isLooping)
            {
                SetLoopBool(particleSystem, false);
            }
            else
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private static void SetLoopBool( ParticleSystem particleSystem, bool value)
    {
        var main = particleSystem.main;
        main.loop = value;
    }
}


[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour , IAbsorbable
{
    private const string MessageSettingsNotSetup = "Settings of object are not setup, please assign a setting.";
    private const float _regenMultiplier = 0.2f;
    [SerializeField] private float speedAbsorbMultiplier = 2f;

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
    [SerializeField] private float forceRequired;
    /// <summary>
    /// The gain of the absorbtion 
    /// </summary>
    [Range(0,1f),SerializeField] private float scaleMultiplier = 0.05f ;

    [SerializeField] protected bool CanRegenerateFromDissolve = true;
    [SerializeField] private bool CanRegenerateScaleFromAbsorbtion = true;
    static float forceTolerance = 0.999f;
    [SerializeField] private bool isDissolvable = true;

    [SerializeField] private bool ignoreForceRequired;
    
    #endregion
    #region Private Variable
    /// <summary>
    /// AudioPlayer used for loop sound etc, cached to be stopped when its desired
    /// </summary>
    private AudioPlayer _audioPlayer;
    private Collider _collider;
    private PlayableVolume _playableVolume;
    private MeshRenderer[] _meshRenderers;
    private List<MaterialPropertyBlockManager> _propBlockManagers;

    //[SerializeField] private Material[] _mtlsDissolvable;
    //[SerializeField] private Material[] _mtlsOpaque;
    private Vector3 _baseScale;
    
    private AudioPlayer _audioPlayerAmbiant;
    private AudioPlayer _audioPlayerAbsorb;
  
    #endregion
    #region Properties
    public Rigidbody Rigidbody { get; private set; }
    public bool IgnoreForceRequired { get => ignoreForceRequired; }
    public float ForceRequired => forceRequired;
    public bool IsAbsorbed { get; set; }
    public bool IsAbsorbable { get; set; }
    public bool IsInAbsorbing { get; set; }
    public Vector3 InitialPosition { get;  set; }
    public int SleepUntilLevel => sleepUntilLevel;
    public float InitialScaleMultiplier { get; private set; }
    public float InitialForceRequired { get; private set; }
    public float ScaleMultiplier => scaleMultiplier;
    public Collider Collider => _collider;
    [SerializeField] private bool _sleepUntilAbsorb;
  

    public float HeightObject
    {
        get
        {
            return Collider.bounds.size.y ;
        }
    }
    public bool SleepUntilAbsorb
    {
        get { return _sleepUntilAbsorb; }
        set { _sleepUntilAbsorb = value; }
    }

    public PlayableVolume PlayableVolume { get; set; }
    #endregion
    
    private Mesh GeneratedCollider;
    private static readonly int Amount = Shader.PropertyToID("_Amount");
    // FX Cached
    protected ParticleSystem[] _onAbsorbFX;
    [SerializeField] private float speedScaleMultiplier = 1;
    private bool hasEnoughForce;

    #region MonoBehaviour

    private void Awake()
    {
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            _collider = transform.AddComponent<MeshCollider>();
        }
        _baseScale = transform.localScale;
        // Generate MaterialPropertyBlock
        _propBlockManagers = new List<MaterialPropertyBlockManager>();
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            var mtlManager = transform.AddComponent<MaterialPropertyBlockManager>();
            mtlManager.Init(_meshRenderers[i]);
            _propBlockManagers.Add(mtlManager);
        }

        Rigidbody = GetComponent<Rigidbody>();
        if (settings == null)
        {
            Debug.LogWarning(MessageSettingsNotSetup, gameObject);
            // Prevent null ref
            settings = ObjectPhysicsScriptableObject.CreateInstance<ObjectPhysicsScriptableObject>();
        }
        InitialScaleMultiplier = ScaleMultiplier;
        InitialForceRequired = forceRequired;

        if (settings.OnAbsorbFX != null)
        {
            _onAbsorbFX = Instantiate(
                settings.OnAbsorbFX._fxPrefab, 
                transform.position, 
                quaternion.identity, null).GetComponentsInChildren<ParticleSystem>();
           
        }
        this.enabled = false;
        AudioManager.PlayLoopSound(settings.OnAmbiantAliaseSound, transform, ref _audioPlayerAmbiant);
    }

    private void OnValidate()
    {
      
    }

    public void Init()
    {
        InitialPosition = transform.position;
        if(_collider is MeshCollider meshCollider)
            meshCollider.convex = true;

        _collider.enabled = false;
        Rigidbody.isKinematic = true;
        Rigidbody.mass = ForceRequired + LevelManager.Instance.DataLevels[SleepUntilLevel].shipScaleAtTheEnd; ;
        LevelManager.Instance.CallbackPreLevelChange += WatchLevelToWakeUp;
        LevelManager.Instance.CallbackLevelChange += GenerateScaleMultiplier;
        LevelManager.Instance.CallbackLevelChange += GenerateForceRequired;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 2)
        {
            if (collision.gameObject.TryGetComponent<CollisionSurface>(out var collisionSurface))
            {
                collisionSurface.Play();
            }
            AudioManager.StopLoopSound(ref _audioPlayerAbsorb);
            AudioManager.PlaySoundAtPosition(settings.OnImpactAliaseSound,transform.position);
            FXManager.PlayFXAtPosition(settings.OnHitFX, transform.position);
            _onAbsorbFX.StopFX(true);
        }
    }


    private void Update()
    {
        if ( PlayableVolume == null)
        {
            if (!IsInAbsorbing)
            {
                _onAbsorbFX.StopFX(false);
                transform.position = InitialPosition;
                Rigidbody.Sleep();
            }
        }
        else
        {
            if(Rigidbody.IsSleeping())
                Rigidbody.WakeUp();
        }
        //if (Rigidbody.velocity.y < 0 && transform.position.y < LevelManager.Instance.Player.transform.position.y-5)
        if (!IsInAbsorbing && transform.position.y < LevelManager.Instance.Player.transform.position.y)
        {
            ChangeMaterialsRenderQueue(3000);
            AudioManager.PlayLoopSound(settings.OnAmbiantAliaseSound, transform, ref _audioPlayerAmbiant);
            
        }

        if (!IsInAbsorbing && Rigidbody.velocity.y > 0)
        {
            _onAbsorbFX.StopFX(false);
            Rigidbody.velocity = new Vector3(Rigidbody.velocity.x,0,Rigidbody.velocity.z);
        }
            
        
    }
    private void OnDestroy()
    {
        if(GeneratedCollider)
            GeneratedCollider.Clear();
        
        _onAbsorbFX.StopFX(false);
        AudioManager.StopLoopSound(ref _audioPlayerAmbiant);
        
    }
    protected void EndObject()
    {
        AudioManager.PlaySoundAtPosition(settings.OnDeathAliaseSound, transform.position);
        //FXManager.PlayFXAtPosition(settings.fxDeath,transform.position);
        //AudioManager.StopLoopSound(ref _audioPlayer);
        LevelManager.Instance.RemoveObjectPhysical(this);
    }

    #if UNITY_EDITOR

    public void EditorChangeMtlOpaque(Material[] mtls)
    {
        //_mtlsOpaque = mtls;
    }
    public void EditorChangeMtlTransparent(Material[] mtls)
    {
        //_mtlsDissolvable = mtls;
    }
        private void OnDrawGizmos()
        {
            // Gizmos.DrawWireSphere(AbsorbePoint.position, 1);
           var LabelStyle = new GUIStyle(GUI.skin.label);
            var LabelStyleRed = new GUIStyle(LabelStyle);
            LabelStyleRed.normal.textColor = Color.red;

            var LabelStyleYellow = new GUIStyle(LabelStyle);
            LabelStyleYellow.normal.textColor = Color.green;
            
            
            var forceRatio = FindObjectOfType<Absorber>().Strenght / ForceRequired;
            bool hasEnoughForce = forceRatio >= forceTolerance;
            if (hasEnoughForce)
            {
                Handles.Label(transform.position, 
                    $"Force Required {ForceRequired} // Gain : {scaleMultiplier}", LabelStyleYellow);
            }
            else
            {
                Handles.Label(transform.position, 
                    $"Force Required {ForceRequired} // Gain : {scaleMultiplier}", LabelStyleRed);
            }
            
        }
    #endif
    #endregion
    private void WatchLevelToWakeUp()
    {
        if ( !IsAbsorbable && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
        {
            LevelManager.Instance.CallbackPreLevelChange -= WatchLevelToWakeUp;
            WakeObject();
        }
    }
    public virtual void WakeObject()
    {
        this.enabled = true;
        _collider.enabled = true;
        LevelManager.Instance.AddObjectPhysical(this);
        IsAbsorbable = true;
        Rigidbody.isKinematic = SleepUntilAbsorb;
       // _meshRenderer.materials = _mtlsDissolvable;
        ChangeMaterialsRenderQueue(3000);
    }
    /// <summary>
    /// Generate the amount of scale to add for the ship, related to the number of object with the scale targeted in the level 
    /// </summary>
    public void GenerateScaleMultiplier()
    {
        float startScale = LevelManager.Instance.ShipStartScale;
        int currentLevel = sleepUntilLevel;
        //var oof = LevelManager.Instance.CurrentObjectList;
        var oof = LevelManager.Instance.GetObjectsInLevel(currentLevel);
        var myObject = LevelManager.Instance;
        float targetScale;
        if (currentLevel >= 1)
        {
            startScale = myObject.DataLevels[currentLevel-1].shipScaleAtTheEnd;
        }
        targetScale = myObject.DataLevels[currentLevel].shipScaleAtTheEnd - startScale;
        float sums = 0;
        foreach (var reward in oof)
        {
            sums += reward.InitialScaleMultiplier;
        }
        float addition = 0;
        addition +=  (InitialScaleMultiplier / sums) * targetScale;
        scaleMultiplier =(float)Math.Round(addition,3) ;
        LevelManager.Instance.CallbackLevelChange -= GenerateScaleMultiplier;
    }
    /// <summary>
    /// Generate the amount of force 
    /// </summary>
    public void GenerateForceRequired()
    {
        if (ignoreForceRequired)
        {
            forceRequired = 0;
        }
        else
        {
            float startScale = LevelManager.Instance.ShipStartScale;
            int currentLevel =  sleepUntilLevel;
            var myObject = LevelManager.Instance;
            if (currentLevel >= 1)
            {
                startScale = myObject.DataLevels[currentLevel-1].shipScaleAtTheEnd;
            }

            if (forceRequired != 0)
            {
                forceRequired = (float)Math.Round(startScale+scaleMultiplier,3);
            }
        }
        LevelManager.Instance.CallbackLevelChange -= GenerateForceRequired;
    }
    protected void SetDissolve(float amount)
    {
        if (!isDissolvable) return;

        for (int i = 0; i < _propBlockManagers.Count; i++)
        {
            _propBlockManagers[i].SetDissolve(amount);
        }
  
    }
    private void AddDissolve(float amount)
    {
        if (!isDissolvable) return;

        for (int i = 0; i < _propBlockManagers.Count; i++)
        {
            _propBlockManagers[i].AddDissolve(amount);
        }
    }
    private bool HasEnoughForce(float strength , out float forceRatio)
    {
        if (forceRequired == 0)
        {
            forceRatio = 1;
        }
        else
        {
            forceRatio = (float)Math.Round(strength / ForceRequired,3);
        }
        return forceRatio >= forceTolerance;
    }
    private void FixedUpdate()
    {
        if ( !IsAbsorbed && IsAbsorbable)
        {
            if (!IsInAbsorbing)
            {
                OnStopAbsorbing();
            }
            else // Absorbtion in progress
            {
                OnIsAbsorbing();
            }
        }

        if (IsAbsorbed)
        {
            OnAbsorbed();
        }
       
    }
    /// <summary>
    /// Update when the object isAbsorbed
    /// </summary>
    protected virtual void OnAbsorbed()
    {
        AddDissolve(Time.deltaTime);
        bool destroyIt = true;
        for (int index = 0; index < _propBlockManagers.Count; index++)
        {
            if(_propBlockManagers[index].FloatsIsLessThan(1))
            {
                destroyIt = false;
            }
        }
        if (destroyIt)
        {
            AudioManager.StopLoopSound(ref _audioPlayerAbsorb);
            Destroy(gameObject);
        }
    }

    protected virtual void OnStopAbsorbing()
    {
        AudioManager.StopLoopSound(ref _audioPlayerAbsorb);
        if (CanRegenerateScaleFromAbsorbtion)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, Time.deltaTime);
        }
        if (CanRegenerateFromDissolve)
        {
            AddDissolve(-Time.deltaTime * _regenMultiplier);
        }
    }
    protected virtual void OnIsAbsorbing()
    {
        AudioManager.StopLoopSound(ref _audioPlayerAmbiant);
        AudioManager.PlayLoopSound(settings.OnMovingAliaseSound, transform, ref _audioPlayerAbsorb);
        if (hasEnoughForce)
        {
            AddDissolve(Time.deltaTime);
        }
    }
    protected virtual void LateUpdate()
    {
        if (!IsAbsorbed)
        {
            // if (IsInAbsorbing)
            // {
            //    
            //     IsInAbsorbing = false;
            // }
        }
    }
    private void ChangeMaterialsRenderQueue(int value)
    {
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            var materials = _meshRenderers[i].materials;
            foreach (var mtl in materials)
            {
                mtl.renderQueue = value;
            }
            _meshRenderers[i].materials = materials; 
        }
       
    }
    public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
    {
        
        IsInAbsorbing = true;
        absorbingState = AbsorbingState.InProgress;
        ChangeMaterialsRenderQueue(3002);
        if(SleepUntilAbsorb)
            Rigidbody.isKinematic = false;
        // Generate direction and apply it to velocity
         hasEnoughForce = HasEnoughForce(absorber.Strenght , out float forceRatio);
        var destination = absorber.AbsorbePoint.position;
        var direction = destination - transform.position;
        direction *= forceRatio;
        Rigidbody.velocity = direction * speedAbsorbMultiplier;
       // transform.position += direction * _speedAbsorbMultiplier *Time.deltaTime;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * speedScaleMultiplier);
        float distanceHeight = destination.y - transform.position.y;
        
        
        _onAbsorbFX.PlayFXAtPosition(true , transform.position);
        
        if(distanceHeight < 5)
        {
            var valueDissolve = 1-(distanceHeight/5);
            if (hasEnoughForce)
            {
                SetDissolve(valueDissolve);
            }
        }
        if (hasEnoughForce && distanceHeight < absorber.AbsortionHeight)
        {
            absorber.Strenght += ScaleMultiplier;
            IsAbsorbed = true;
            EndObject();
            absorbingState = AbsorbingState.Done;
        }
        else if (!hasEnoughForce && distanceHeight < absorber.AbsortionHeight)
        {
            absorbingState = AbsorbingState.Fail;
        }
        else if (!hasEnoughForce)
        {
            absorber.Ship.transform.position += -direction * forceRatio * 2f * Time.deltaTime;
        }
    }
    public bool OnTrigger(Absorber absorber)
    {
        if (!HasEnoughForce(absorber.Strenght, out var forceRatio))
        {
            var destination = absorber.AbsorbePoint.position;
            var direction = destination - transform.position;
            // absorber.Ship.transform.position += -direction * forceRatio * .2f * Time.deltaTime;
            return false;
        }
        else
        {
            // var destination = absorber.AbsorbePoint.position;
            // var direction = destination - transform.position;
            //
            // absorber.Ship.transform.position += (-Vector3.down * HeightObject)* forceRatio * .2f * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position,
            //HeightObject + LevelManager.Instance.GetCurrentHeightOffset, Time.deltaTime * speedHeightMove);
        }
        return true;
    }

}
