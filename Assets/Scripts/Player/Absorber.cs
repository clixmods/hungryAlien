using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioAliase;


public class Absorber : MonoBehaviour
{
    private Light _light;
    private CapsuleCollider _collider;
    
    /// <summary>
    /// Strenght applied when the player absorb object
    /// </summary>
    [SerializeField] private float strenght = 1;
    /// <summary>
    /// Used to influence the height of the ship and the radius of the light
    /// </summary>
    [SerializeField] private float radius = 3;

    [SerializeField] private List<GameObject> inTheTrigger;
    public List<GameObject> InTheTrigger => inTheTrigger;

    [SerializeField] Transform AbsorbePoint;
    [SerializeField] ScaleShip scaleShip;
    
    [SerializeField] float scaleMultiplier = 1.1f;
    //[SerializeField] float strengthMultiplier = 1.1f;

    private const float FailedCooldownStart = 2;
    private float _failedCooldown;
    
    /// <summary>
    /// Looped sound played for the light
    /// </summary>
    [Header("Sound Aliases"),SerializeField,Aliase] private string aliaseLoopLight;
    /// <summary>
    /// Looped sound played when the player start a absorption
    /// </summary>
    [SerializeField,Aliase] private string aliaseLoopAbsorbing;
    /// <summary>
    /// Looped sound played when the player fail an absorption
    /// </summary>
    [SerializeField,Aliase] private string aliaseLoopFail;
    [SerializeField,Aliase] private string aliaseAbsorbSuccess;
    private AudioPlayer _audioPlayer;
    private AudioPlayer _audioPlayerLightLoop;
    private AudioPlayer _audioPlayerFail;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponentInChildren<Light>();
        _collider = GetComponentInChildren<CapsuleCollider>();
        scaleShip = transform.parent.GetComponent<ScaleShip>();
        
        _collider.direction = 2;
    }

    // Update is called once per frame
    void Update()
    {
        AudioManager.PlayLoopSound(aliaseLoopLight,transform,ref _audioPlayerLightLoop);
        
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

        if (Mouse.current.leftButton.isPressed && _failedCooldown <= 0)
        {
            AudioManager.PlayLoopSound(aliaseLoopAbsorbing, transform, ref _audioPlayer);
        }
        else
            AudioManager.StopLoopSound(ref _audioPlayer);
        
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<ObjectPhysics>(out var objectPhysics))
        {
                if(!inTheTrigger.Contains(other.gameObject))
                    inTheTrigger.Add(other.gameObject);

                if (objectPhysics.IsAbsorbed) return;
                float forceRemaining = strenght / objectPhysics.ForceRequired;
                
                if (Mouse.current.leftButton.isPressed && _failedCooldown <= 0)
                {
                    Rigidbody rb = objectPhysics.ObjectRigidbody;
                    var destination = AbsorbePoint.position;
                    var direction = (destination - other.transform.position);
                    direction *= (forceRemaining) ;
                    
                   // rb.AddForce(direction * strenght);
                    //rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, 5);
                    rb.velocity = direction;

                    if (forceRemaining < 1)
                    {
                        scaleShip.enabled = false;
                        scaleShip.transform.position += -direction * forceRemaining * Time.deltaTime;
                    }
                    // Ship can absorb
                    if (forceRemaining >= 1 && destination.y - other.transform.position.y < 2f * scaleShip.GetScaleFactor()/2)
                    {
                        Destroy(other.gameObject);
                        scaleShip.SetScaleFactor(objectPhysics.ScaleMultiplier);
                        Debug.Log($"[Absorber] Gain {objectPhysics.ScaleMultiplier}  ==> {scaleShip.GetScaleFactor()}");
                        strenght += objectPhysics.ScaleMultiplier;
                        AudioManager.PlaySoundAtPosition(aliaseAbsorbSuccess, transform.position);
                        objectPhysics.IsAbsorbed = true;
                    }
                    else if (forceRemaining < 1 && destination.y - other.transform.position.y < 3f * scaleShip.GetScaleFactor()/2)
                    {
                        CameraShake.SetNoisier(1,1);
                        AudioManager.PlayLoopSound(aliaseLoopFail, transform, ref _audioPlayerFail);
                        _failedCooldown = FailedCooldownStart;
                    }
                }
                else
                {
                    scaleShip.enabled = true;
                }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(inTheTrigger.Contains(other.gameObject))
            inTheTrigger.Remove(other.gameObject);
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(AbsorbePoint.position, 1);
        Handles.Label(transform.position, "Force :"+strenght );
    }
}