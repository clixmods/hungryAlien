using System;
using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayableVolume : MonoBehaviour
{
    private Collider _collider;
    private bool _triggerStayCheck;

    private List<IAbsorbable> _absorbableList;
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
            for (int i = 0; i < _absorbableList.Count; i++)
            {
                _absorbableList[i].transform.position = LevelManager.Instance.CurrentPlayerSpawnPoint.position;
            }
        }
        
    }

    public void Activate()
    {
        _triggerStayCheck = true;

        _absorbableList = new List<IAbsorbable>();
        _absorbableList.AddRange(LevelManager.Instance.CurrentObjectList);
        // Bug apparently, this line do a ref of the list and not a copy
        //_objectPhysicsList = LevelManager.Instance.CurrentObjectList; 

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IAbsorbable>(out var objectAbsorbable) && objectAbsorbable.IsAbsorbable)
        {
            other.transform.position = LevelManager.Instance.CurrentPlayerSpawnPoint.position;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_triggerStayCheck)
        {
            if (other.TryGetComponent<IAbsorbable>(out var objectAbsorbable))
            {
                if (_absorbableList.Contains(objectAbsorbable))
                {
                    _absorbableList.Remove(objectAbsorbable);
                }
            }
        }
    }
}
