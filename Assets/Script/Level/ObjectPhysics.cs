using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[SelectionBase]
public class ObjectPhysics : MonoBehaviour
{
    [SerializeField] private int sleepUntilLevel;

    private Rigidbody _rb;



    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        WatchLevel();
    }

    void WatchLevel()
    {
        if ( !_rb.isKinematic && LevelManager.Instance.CurrentLevel == sleepUntilLevel)
        {
            _rb.isKinematic = true;
            LevelManager.Instance.AddObjectPhysical(this);
        }
    }

    private void OnDestroy()
    {
        LevelManager.Instance.RemoveObjectPhysical(this);
    }
}
