using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;

[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour
{
    [SerializeField] private int sleepUntilLevel;

    private Rigidbody _rb;

    [Range(0,10)]
    [SerializeField] private float magnitudeToStopLoop = 0;

    [Range(1,2)]
    [SerializeField] private float scaleMultiplier = 1.05f ;

    public float ScaleMultiplier
    {
        get { return scaleMultiplier; }
    }



    [Header("Sound Aliases")]
    [SerializeField][Aliase] string aliaseAmbiant;
    [SerializeField][Aliase] string aliaseMoving;
    [SerializeField][Aliase] string aliaseDeath;

    private AudioPlayer _audioPlayer;
    
    
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
        if (_rb.velocity.magnitude > magnitudeToStopLoop)
        {
            AudioManager.PlayLoopSound(aliaseMoving, transform.position, ref _audioPlayer);
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
        if ( _rb.isKinematic && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
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
