using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioAliase;
using UnityEngine.Windows;
using Level;
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
    [SerializeField] ScaleShip scaleShip;

    public Transform Ship => scaleShip.transform;

    //[SerializeField] float scaleMultiplier = 1.1f; // We use a value managed by the absorbedObject
    //[SerializeField] float strengthMultiplier = 1.1f;

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

        _collider.direction = 2;
        Input.Game.Absorb.performed += ctx => Absorb();
        Input.Game.Absorb.canceled += ctx => CancelAbsorb();

    }

    // Update is called once per frame
    void Update()
    {
        AudioManager.PlayLoopSound(aliaseLoopLight, transform, ref _audioPlayerLightLoop);

        _light.range = radius;
        _collider.height = radius * 2;
        Vector3 centerCollider = _collider.center;
        centerCollider.z = radius;
        _collider.center = centerCollider;

        if (_failedCooldown > 0)
            _failedCooldown -= Time.deltaTime;
        else
        {
            AudioManager.StopLoopSound(ref _audioPlayerFail);
        }

        if (absorb && _failedCooldown <= 0)
        {
            AudioManager.PlayLoopSound(aliaseLoopAbsorbing, transform, ref _audioPlayer);
        }
        else
            AudioManager.StopLoopSound(ref _audioPlayer);

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
                // float forceRemaining = strenght / objectPhysics.ForceRequired;
                // Rigidbody rb = objectPhysics.ObjectRigidbody;
                // var destination = AbsorbePoint.position;
                // var direction = (destination - other.transform.position);
                // direction *= (forceRemaining);
                // rb.velocity = direction;
                objectPhysics.OnAbsorb(this, out AbsorbingState absorbingState);
                switch (absorbingState)
                {
                    case AbsorbingState.Start:
                        break;
                    case AbsorbingState.InProgress:
                        break;
                    case AbsorbingState.Done:
                        AudioManager.PlaySoundAtPosition(aliaseAbsorbSuccess, transform.position);
                        VEhasAbsorb.Play();
                        break;
                    case AbsorbingState.Fail:
                        _failedCooldown = FailedCooldownStart;
                        CameraShake.SetNoisier(1, 1);
                        AudioManager.PlayLoopSound(aliaseLoopFail, transform, ref _audioPlayerFail);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // // Ship can absorb
                // if (forceRemaining >= 1 &&
                //     destination.y - other.transform.position.y < 2f * scaleShip.GetScaleFactor() / 2)
                // {
                //     Destroy(other.gameObject);
                //     scaleShip.SetScaleFactor(objectPhysics.ScaleMultiplier);
                //     Debug.Log($"[Absorber] Gain {objectPhysics.ScaleMultiplier}  ==> {scaleShip.GetScaleFactor()}");
                //     strenght += objectPhysics.ScaleMultiplier;
                //     AudioManager.PlaySoundAtPosition(aliaseAbsorbSuccess, transform.position);
                //     objectPhysics.IsAbsorbed = true;
                //     VEhasAbsorb.Play();
                // }
                // else if (forceRemaining < 1 &&
                //          destination.y - other.transform.position.y < 3f * scaleShip.GetScaleFactor() / 2)
                // {
                //     CameraShake.SetNoisier(1, 1);
                //     AudioManager.PlayLoopSound(aliaseLoopFail, transform, ref _audioPlayerFail);
                //     _failedCooldown = FailedCooldownStart;
                // }
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
            inTheTrigger.Remove(other.gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(absorbePoint.position, 1);
        Handles.Label(transform.position, "Force :" + strenght);
    }
}