using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;

[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour
{
    public ObjectPhysicsScriptableObject _settings;
 
    private Rigidbody _rb;
    private AudioPlayer _audioPlayer;
    
    public float ScaleMultiplier
    {
        get { return _settings.scaleMultiplier; }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        WatchLevel();
        if (_rb.velocity.magnitude > _settings.magnitudeToStopLoop)
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
        if ( _rb.isKinematic && LevelManager.Instance.CurrentLevel == _settings.sleepUntilLevel)
        {
            _rb.isKinematic = false;
            LevelManager.Instance.AddObjectPhysical(this);
        }
    }

    private void OnDestroy()
    {
      
        LevelManager.Instance.RemoveObjectPhysical(this);
    }
}
