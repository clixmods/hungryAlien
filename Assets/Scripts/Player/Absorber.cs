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
    [SerializeField] private float strenght = 1;
    [SerializeField] private float radius= 3;

    [SerializeField] private List<GameObject> inTheTrigger;
    public List<GameObject> InTheTrigger
    {
        get { return inTheTrigger; }
    }

    [SerializeField] Transform AbsorbePoint;
    [SerializeField] ScaleShip scaleShip;
    [SerializeField] float scaleMultiplier = 1.1f;
    [SerializeField] float strengthMultiplier = 1.1f;

    private const float FailedCooldownStart = 2;
    private float failedCooldown;
    
    [Header("Sound Aliases")]
    [Aliase] public string aliaseLoopLight;
    [Aliase] public string aliaseLoopAbsorbing;
    [Aliase] public string aliaseLoopFail;
    [Aliase] public string aliaseAbsorbReleased;
    [Aliase] public string aliaseAbsorbSuccess;
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

        if (failedCooldown > 0)
            failedCooldown -= Time.deltaTime;
        else
        {
            AudioManager.StopLoopSound(ref _audioPlayerFail);
        }

        if (Mouse.current.leftButton.isPressed && failedCooldown <= 0)
        {
            AudioManager.PlayLoopSound(aliaseLoopAbsorbing, transform, ref _audioPlayer);
        }
        else
            AudioManager.StopLoopSound(ref _audioPlayer);


    }

    private void FixedUpdate()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
       
            if (other.TryGetComponent<ObjectPhysics>(out ObjectPhysics objectPhysics))
            {
                if(!inTheTrigger.Contains(other.gameObject))
                    inTheTrigger.Add(other.gameObject);

                float forceRemaining = strenght / objectPhysics.ForceRequired;
                
                if (Mouse.current.leftButton.isPressed && failedCooldown <= 0)
                {
                    
                    Rigidbody rb = objectPhysics.ObjectRigidbody;
                    Vector3 destination = AbsorbePoint.position;
                    
                 
                    Vector3 direction = (destination - other.transform.position);
                    direction = direction * (forceRemaining) ;
                    
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
                        scaleShip.SetScaleFactor(scaleShip.GetScaleFactor() * scaleMultiplier);
                        strenght *= objectPhysics.ScaleMultiplier;
                        AudioManager.PlaySoundAtPosition(aliaseAbsorbSuccess, transform.position);
                    }
                    else if (forceRemaining < 1 && destination.y - other.transform.position.y < 3f * scaleShip.GetScaleFactor()/2)
                    {
                        CameraShake.SetNoisier(1,1);
                        AudioManager.PlayLoopSound(aliaseLoopFail, transform, ref _audioPlayerFail);
                        failedCooldown = FailedCooldownStart;
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