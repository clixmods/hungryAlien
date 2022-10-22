using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*

static int currentLevel 
Waypoint[] : List des points ou la caméra sera posé, la taille dépendra de currentLevel
*/
public class LevelManager : MonoBehaviour
{
    [SerializeField] private float _speedMoveCamera = 10;
    private Camera _camera; 
    
    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get { return _instance; }
    }
    [SerializeField] Transform[] waypoints;
    private int _currentLevel;
    public int CurrentLevel
    {
        get { return _currentLevel; }
    }

    private List<ObjectPhysics> _objectPhysicsList;
    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
        _objectPhysicsList = new List<ObjectPhysics>();

        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
       
        Transform camTrans = _camera.transform;
        Transform wpTransform = waypoints[_currentLevel].transform;
        if (camTrans.position != wpTransform.position ||  camTrans.rotation != wpTransform.rotation)
        {
            camTrans.position = Vector3.MoveTowards(camTrans.position, wpTransform.position,
                Time.deltaTime * _speedMoveCamera);
            camTrans.rotation = Quaternion.RotateTowards(camTrans.rotation , wpTransform.rotation, Time.deltaTime * _speedMoveCamera);
            
        }
        else
        {
            WatchObjectPhysicalAvailable();
        }
    }

    private void WatchObjectPhysicalAvailable()
    {
        bool rip = true;
        foreach(var myObject in _objectPhysicsList)
        {
            if (myObject != null)
            {
                rip = false;
                break;
            }
        }

        if (rip)
        {
            _currentLevel++;
        }
    }

    public void AddObjectPhysical(ObjectPhysics objectPhysics)
    {
        _objectPhysicsList.Add(objectPhysics);
    }
    public void RemoveObjectPhysical(ObjectPhysics objectPhysics)
    {
        _objectPhysicsList.Remove(objectPhysics);
    }
    
}
