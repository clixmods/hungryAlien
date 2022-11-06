using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayableVolume : MonoBehaviour
{
    private Collider _collider;
    private bool _triggerStayCheck;

    private List<ObjectPhysics> _objectPhysicsList;
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_triggerStayCheck)
        {
            _triggerStayCheck = false;
            for (int i = 0; i < _objectPhysicsList.Count; i++)
            {
                _objectPhysicsList[i].transform.position = LevelManager.Instance.CurrentPlayerSpawnPoint.position;
            }
        }
        
    }

    public void Activate()
    {
        _triggerStayCheck = true;

        _objectPhysicsList = new List<ObjectPhysics>();
        _objectPhysicsList.AddRange(LevelManager.Instance.CurrentObjectList);
        //_objectPhysicsList = LevelManager.Instance.CurrentObjectList; // Bug apparently, this line do a ref of the list and not a copy

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ObjectPhysics>(out var objectPhysics) && objectPhysics.IsGrabable)
        {
            other.transform.position = LevelManager.Instance.CurrentPlayerSpawnPoint.position;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_triggerStayCheck)
        {
            if (other.TryGetComponent<ObjectPhysics>(out var objectPhysics))
            {
                if (_objectPhysicsList.Contains(objectPhysics))
                {
                    _objectPhysicsList.Remove(objectPhysics);
                }
            }
        }
    }
}
