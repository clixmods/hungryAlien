using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
using FX;
using Unity.VisualScripting;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour
{
    public ObjectPhysicsScriptableObject _settings;
 
    #region Private Variable
    
    public Rigidbody ObjectRigidbody { get; private set; }
    private AudioPlayer _audioPlayer;
    private MeshCollider _collider;
    #endregion
    public GameObject placeholder;
    
    public float ScaleMultiplier
    {
        get { return _settings.scaleMultiplier; }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<MeshCollider>();
        if (_collider == null)
            _collider = transform.AddComponent<MeshCollider>();
        
        _collider.convex = true;
        ObjectRigidbody = GetComponent<Rigidbody>();
        ObjectRigidbody.isKinematic = true;
        ObjectRigidbody.mass = _settings.ForceRequired;
        if (_settings == null)
        {
            Debug.Log("Settings object Physics not setup", gameObject);
            gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        ObjectRigidbody.mass = _settings.ForceRequired; // TODO : TEMP
        WatchLevel();
        if (ObjectRigidbody.velocity.magnitude > _settings.magnitudeToStopLoop)
        {
            AudioManager.PlayLoopSound(_settings.aliaseMoving, transform.position, ref _audioPlayer);
        }
        else
        {
            if (_audioPlayer != null)
            {
                AudioManager.StopLoopSound(_audioPlayer);
                _audioPlayer = null;
            }
        }
    }

    void WatchLevel()
    {
        if ( ObjectRigidbody.isKinematic && LevelManager.Instance.CurrentLevel == _settings.sleepUntilLevel)
        {
            ObjectRigidbody.isKinematic = false;
            LevelManager.Instance.AddObjectPhysical(this);
        }
    }

    private void OnDestroy()
    {
//        FXManager.PlayFXAtPosition(_settings.fxDeath,transform.position);
        AudioManager.StopLoopSound(_audioPlayer);
        LevelManager.Instance.RemoveObjectPhysical(this);
    }
    
    private void OnDrawGizmos()
    {
       // Gizmos.DrawWireSphere(AbsorbePoint.position, 1);
        Handles.Label(transform.position, "Force Required :"+_settings.ForceRequired );
    }
}
