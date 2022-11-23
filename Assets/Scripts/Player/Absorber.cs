using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioAliase;
using UnityEngine.Windows;
using Level;
using Player;
using UnityEngine.VFX;


public enum AbsorbingState
{
    Start,
    InProgress,
    Done,
    Fail
}

public class Absorber : MonoBehaviour
{
    private Light _light;
    private CapsuleCollider _collider;

    private AbsorberColor _absorberColor;
    [SerializeField] private Color colorIdle = Color.white;
    [SerializeField] private Color colorIsAbsorbing = Color.cyan;
    [SerializeField] private Color colorFailed = Color.red;

    [SerializeField] private ParticleSystem particleSystemVaccum;
    /// <summary>
    /// Strenght applied when the player absorb object
    /// </summary>
    [SerializeField] private float strenght = 1;
    public float Strenght
    {
        get { return strenght; }
        set
        {
            scaleShip.SetScaleFactor(value-strenght);
            strenght = value;
        }
    }

    /// <summary>
    /// Used to influence the height of the ship and the radius of the light
    /// </summary>
    [SerializeField] private float radius = 3;
    [SerializeField] private List<GameObject> inTheTrigger;
    public List<GameObject> InTheTrigger => inTheTrigger;
    [SerializeField] Transform absorbePoint;
    public Transform AbsorbePoint => absorbePoint;
    [SerializeField] private ScaleShip scaleShip;
    public Transform Ship => scaleShip.transform;
   
    private const float FailedCooldownStart = 2;
    private float _failedCooldown;

    /// <summary>
    /// Looped sound played for the light
    /// </summary>
    [Header("Sound Aliases"), SerializeField, Aliase]
    private string aliaseLoopLight;

    /// <summary>
    /// Looped sound played when the player start a absorption
    /// </summary>
    [SerializeField, Aliase] private string aliaseLoopAbsorbing;

    /// <summary>
    /// Looped sound played when the player fail an absorption
    /// </summary>
    [SerializeField, Aliase] private string aliaseLoopFail;

    [SerializeField, Aliase] private string aliaseAbsorbSuccess;
    private AudioPlayer _audioPlayer;
    private AudioPlayer _audioPlayerLightLoop;
    private AudioPlayer _audioPlayerFail;
    private InputAsset _input;
    public InputAsset Input
    {
        get
        {
            // Prevent null ref when the game reload script
            if (_input == null)
            {
                _input = new InputAsset();
            }
            return _input;
        }
    }
    private bool absorb;
    

    [SerializeField] private VisualEffect VEhasAbsorb;

    public float AbsortionHeight => 2f * scaleShip.GetScaleFactor() / 2; 
    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponentInChildren<Light>();
        _collider = GetComponentInChildren<CapsuleCollider>();
        scaleShip = transform.parent.GetComponent<ScaleShip>();
        
        _absorberColor = GetComponentInChildren<AbsorberColor>();

        _collider.direction = 2;
        Input.Game.Absorb.performed += ctx => Absorb();
        Input.Game.Absorb.canceled += ctx => CancelAbsorb();

    }

    // Update is called once per frame
    void Update()
    {
        AudioManager.PlayLoopSound(aliaseLoopLight, transform, ref _audioPlayerLightLoop);
        _light.range = radius * Ship.localScale.magnitude;
        _collider.height = radius * 2;
        Vector3 centerCollider = _collider.center;
        centerCollider.z = radius;
        _collider.center = centerCollider;
        if (_failedCooldown > 0)
        {
            _absorberColor.color = colorFailed;
            _failedCooldown -= Time.deltaTime;
        }
        else
        {
            AudioManager.StopLoopSound(ref _audioPlayerFail);
        }
        if (absorb && _failedCooldown <= 0)
        {
            if(!particleSystemVaccum.isPlaying)
                particleSystemVaccum.Play();
            
            AudioManager.PlayLoopSound(aliaseLoopAbsorbing, transform, ref _audioPlayer);
        }
        else
        {
            particleSystemVaccum.Stop();
            AudioManager.StopLoopSound(ref _audioPlayer);
        }
        if(LevelManager.Instance.State == GameState.Ingame)
        {
            Input.Enable();
        }
        else
        {
            Input.Disable();
            absorb = false;
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<IAbsorbable>(out var objectPhysics))
        {
            if (!objectPhysics.IsAbsorbable)
            {
                return;
            }
            if (!inTheTrigger.Contains(other.gameObject))
                inTheTrigger.Add(other.gameObject);

            if (objectPhysics.IsAbsorbed) return;
            if (absorb && _failedCooldown <= 0)
            {
                objectPhysics.OnAbsorb(this, out AbsorbingState absorbingState);
                switch (absorbingState)
                {
                    case AbsorbingState.Start:
                        
                        break;
                    case AbsorbingState.InProgress:
                        _absorberColor.color = colorIsAbsorbing;
                        break;
                    case AbsorbingState.Done:
                        _absorberColor.color = colorIdle;
                        AudioManager.PlaySoundAtPosition(aliaseAbsorbSuccess, transform.position);
                        VEhasAbsorb.Play();
                        break;
                    case AbsorbingState.Fail:
                        _absorberColor.color = colorFailed;
                        _failedCooldown = FailedCooldownStart;
                        CameraShake.SetNoisier(1, 1);
                        AudioManager.PlayLoopSound(aliaseLoopFail, transform, ref _audioPlayerFail);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                scaleShip.enabled = true;
            }
        }
    }
    public void Absorb()
    {
        absorb = true;
    }
    public void CancelAbsorb()
    {
        absorb = false;
    }
    private void OnTriggerExit(Collider other)
    {
        if (inTheTrigger.Contains(other.gameObject))
        {
            inTheTrigger.Remove(other.gameObject);
            _absorberColor.color = colorIdle;
        }
    }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(absorbePoint.position, 1);
            Handles.Label(transform.position, "Force :" + strenght);
        }
    #endif
    
}