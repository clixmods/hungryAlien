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
    [SerializeField,Range(0f, 1)] private float forceRequired;
    /// <summary>
    /// The gain of the absorbtion 
    /// </summary>
    [Range(0,1f),SerializeField] private float scaleMultiplier = 0.05f ;

    [SerializeField] protected bool CanRegenerateFromDissolve = true;

    #endregion
    #region Private Variable
    /// <summary>
    /// AudioPlayer used for loop sound etc, cached to be stopped when its desired
    /// </summary>
    private AudioPlayer _audioPlayer;
    private Collider _collider;
    private PlayableVolume _playableVolume;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock[] _propBlocks;
    private Vector3 _baseScale;
  
    #endregion
    #region Properties
    public Rigidbody Rigidbody { get; private set; }
    public float ForceRequired => forceRequired;
    public bool IsAbsorbed { get; set; }
    public bool IsAbsorbable { get; set; }
    
    public bool IsInAbsorbing { get; set; }
    
    public Vector3 InitialPosition { get;  set; }
    
    public int SleepUntilLevel => sleepUntilLevel;

    public float InitialScaleMultiplier { get; private set; }
    public float ScaleMultiplier => scaleMultiplier;

    [SerializeField] private bool _sleepUntilAbsorb;

    public bool SleepUntilAbsorb
    {
        get { return _sleepUntilAbsorb; }
        set { _sleepUntilAbsorb = value; }
    }

    public PlayableVolume PlayableVolume { get; set; }
    #endregion


    private Mesh GeneratedCollider;
    #region MonoBehaviour

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _baseScale = transform.localScale;
        _propBlocks = new MaterialPropertyBlock[_meshRenderer.materials.Length];
        for (int i = 0; i < _propBlocks.Length; i++)
        {
            _propBlocks[i] = new MaterialPropertyBlock();
        }
        _collider = GetComponent<Collider>();
        if (_collider == null)
        {
            _collider = transform.AddComponent<MeshCollider>();
        }

        if (_collider is MeshCollider meshCollider && meshCollider.sharedMesh == null)
        {
            
            transform.localScale = Vector3.one;
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int index = 0;
            while (index < meshFilters.Length)
            {
                combine[index].mesh = meshFilters[index].sharedMesh;
                var bound = new Bounds();
                var ogpos = meshFilters[index].transform.position; 
                meshFilters[index].transform.position = Vector3.zero;
                combine[index].transform = meshFilters[index].transform.localToWorldMatrix  ;
                bound.size = meshFilters[index].transform.localScale;
                combine[index].mesh.bounds = bound;
                meshFilters[index].transform.position = ogpos;
                index++;
            }
            
             GeneratedCollider = new Mesh();
             GeneratedCollider.CombineMeshes(combine);
             meshCollider.sharedMesh = GeneratedCollider;
             transform.localScale = _baseScale;
        }
        
       

        Rigidbody = GetComponent<Rigidbody>();
         
        if (settings == null)
        {
            Debug.LogWarning(MessageSettingsNotSetup, gameObject);
            // Prevent null ref
            settings = ObjectPhysicsScriptableObject.CreateInstance<ObjectPhysicsScriptableObject>(); 
           
        }

        InitialScaleMultiplier = ScaleMultiplier;
    }

    public void Init()
    {
        InitialPosition = transform.position;
        if(_collider is MeshCollider meshCollider)
            meshCollider.convex = true;
        
        _collider.enabled = false;
        
        Rigidbody.isKinematic = true;
        Rigidbody.mass = ForceRequired;
        

        LevelManager.Instance.CallbackPreLevelChange += WatchLevelToWakeUp;
        LevelManager.Instance.CallbackLevelChange += GenerateScaleMultiplier;

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

        
        if (Rigidbody.velocity.y < 0 && transform.position.y < LevelManager.Instance.Player.transform.position.y-5)
        {
           
                var materials = _meshRenderer.materials;
                foreach (var mtl in materials)
                {
                    mtl.renderQueue = 3000;
                }
                _meshRenderer.materials = materials; 
            
        }
            
    }

    private void OnDestroy()
    {
        if(GeneratedCollider)
            GeneratedCollider.Clear();
    }

    protected void EndObject()
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
     private void WatchLevelToWakeUp()
    {
        if ( !IsAbsorbable && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
        {
            LevelManager.Instance.CallbackPreLevelChange -= WatchLevelToWakeUp;
            WakeObject();
        }
    }

    protected virtual void WakeObject()
    {
        _collider.enabled = true;
        LevelManager.Instance.AddObjectPhysical(this);
        IsAbsorbable = true;
        Rigidbody.isKinematic = SleepUntilAbsorb;
        ChangeMaterialsRenderQueue(3000);
    }
    /// <summary>
    /// Generate the amount of scale to add for the ship, related to the number of object with the scale targeted in the level 
    /// </summary>
    public void GenerateScaleMultiplier()
    {
        float startScale = 1f;
        
        int currentLevel = LevelManager.Instance.CurrentLevel;
        var oof = LevelManager.Instance.CurrentObjectList;
        var myObject = LevelManager.Instance;
        float targetScale;
        if (currentLevel >= 1)
        {
            startScale = myObject.DataLevels[currentLevel-1].shipScaleAtTheEnd;
            targetScale = myObject.DataLevels[currentLevel].shipScaleAtTheEnd - startScale;
        }
        targetScale = myObject.DataLevels[currentLevel].shipScaleAtTheEnd - startScale;
        float sums = 0;
        foreach (var reward in oof)
        {
            sums += reward.InitialScaleMultiplier;
        }
        float addition = 0;
        addition +=  (InitialScaleMultiplier / sums) * targetScale;
        scaleMultiplier = addition;
    }

    protected void SetDissolve(float amount)
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

    private void AddDissolve(float amount)
    {
        for (int i = 0; i < _propBlocks.Length; i++)
        {
            // Get the current value of the material properties in the renderer.
            _meshRenderer.GetPropertyBlock(_propBlocks[i], i);
            float currentAmount = _propBlocks[i].GetFloat("_Amount");
            //Assign our new value.
            _propBlocks[i].SetFloat("_Amount", currentAmount + amount);
            // Apply the edited values to the renderer.
            _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
        }
    }

    private void FixedUpdate()
    {
        if (IsAbsorbable)
        {
            if (!IsInAbsorbing)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, Time.deltaTime);
                if (CanRegenerateFromDissolve)
                {
                    AddDissolve(-Time.deltaTime * _regenMultiplier);
                }
            }
            else // Absorbtion in progress
            {
                AddDissolve(Time.deltaTime);
            }

            if (IsAbsorbed)
            {
                bool destroyIt = true;
                for (int i = 0; i < _propBlocks.Length ; i++)
                {
                    _meshRenderer.GetPropertyBlock(_propBlocks[i],i);
                    if (_propBlocks[i].GetFloat("_Amount") < 1)
                    {
                        destroyIt = false;
                    }
                    
                    _meshRenderer.SetPropertyBlock(_propBlocks[i], i);
                }
                if(destroyIt)
                    Destroy(gameObject);
            }
        }
       
    }

    private void LateUpdate()
    {
        if(!IsAbsorbed)
            IsInAbsorbing = false;
    }

    private void ChangeMaterialsRenderQueue(int value)
    {
        var materials = _meshRenderer.materials;
        foreach (var mtl in materials)
        {
            mtl.renderQueue = value;
        }
        _meshRenderer.materials = materials; 
    }
    public virtual void OnAbsorb(Absorber absorber, out AbsorbingState absorbingState)
    {
        IsInAbsorbing = true;
        absorbingState = AbsorbingState.InProgress;
        ChangeMaterialsRenderQueue(3002);
        Rigidbody.isKinematic = false;
        // Generate direction and apply it to velocity
        var forceRatio = absorber.Strenght / ForceRequired;
        var destination = absorber.AbsorbePoint.position;
        var direction = destination - transform.position;
        direction *= forceRatio;
        Rigidbody.velocity = direction;
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime);
        bool hasEnoughForce = forceRatio >= 1;
        float distanceHeight = destination.y - transform.position.y;
        if(distanceHeight < 5) 
        { 
            SetDissolve(1-(distanceHeight/5));
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
            absorber.Ship.transform.position += -direction * forceRatio * Time.deltaTime;
        }
        
    }
}
