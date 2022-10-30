using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
/*
static int currentLevel 
Waypoint[] : List des points ou la caméra sera posé, la taille dépendra de currentLevel
*/

public enum GameState
{
    Ingame,
    CameraIsMoving,
    Endgame
    
}
[RequireComponent(typeof(CinemachineSmoothPath))]
public class LevelManager : MonoBehaviour
{
    [SerializeField] private float _speedMoveCamera = 10;
    private Camera _camera;

    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("LevelManager").AddComponent<LevelManager>();
            }

            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    [field : SerializeField] public int CurrentLevel { get; private set; }
    public GameState State { get; private set; }
    public List<Transform> waypoints;
    public List<GameObject> floorCollision;
    private CinemachineSmoothPath _smoothPath;
    private CinemachineDollyCart _dollyCart;
    private CinemachineVirtualCamera _virtualCamera;
    private ShipController _player;
    public int MaxLevel
    {
        get { return waypoints.Count; }
    }
    private List<ObjectPhysics> _objectPhysicsList;
    public GameObject GetCurrentFloor
    {
        get { return floorCollision[CurrentLevel]; }
    }
  
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _objectPhysicsList = new List<ObjectPhysics>();
        _camera = Camera.main;
        _smoothPath = GetComponent<CinemachineSmoothPath>();
        _player = GameObject.FindObjectOfType<ShipController>();
        if (!Camera.main.TryGetComponent<CinemachineBrain>(out CinemachineBrain cinemachineBrain))
        {
            Camera.main.AddComponent<CinemachineBrain>();
        }



        if (_virtualCamera == null)
        {
            var cm = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            if (cm == null)
            {
                var gameObjectfdp = Instantiate(new GameObject());
                _virtualCamera = gameObjectfdp.AddComponent<CinemachineVirtualCamera>();
            }
            else
            {
                _virtualCamera = cm;
            }
        }

        
        
        if (_virtualCamera.TryGetComponent<CinemachineDollyCart>(out CinemachineDollyCart component))
        {
            _dollyCart = component;
        }
        else
        {
            _dollyCart = _virtualCamera.AddComponent<CinemachineDollyCart>();
        }

        _dollyCart.m_Path = _smoothPath;
        _dollyCart.m_PositionUnits = CinemachinePathBase.PositionUnits.PathUnits;

        foreach (var VARIABLE in floorCollision)
        {
            VARIABLE.layer = LayerMask.NameToLayer("MoveZone");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_dollyCart.m_Position <= CurrentLevel)
        {
            foreach (var floorTransform in floorCollision)
            {
                floorTransform.gameObject.SetActive(false);
            }
            if(_objectPhysicsList[0] != null)
                _virtualCamera.LookAt = _objectPhysicsList[0].transform;
            else
            {
                _virtualCamera.LookAt = null;
            }
            
            _dollyCart.m_Speed = _speedMoveCamera;
            State = GameState.CameraIsMoving;
        }
        else
        {
            _virtualCamera.LookAt = _player.transform;
            _dollyCart.m_Speed = 0;
            State = GameState.Ingame;
            floorCollision[CurrentLevel].gameObject.SetActive(true);
            WatchObjectPhysicalAvailable();
        }
        
        
        // if (_camera != null && (waypoints != null && MaxLevel != 0) )
        // {
        //     Transform camTrans = _camera.transform;
        //     Transform wpTransform = waypoints[CurrentLevel].transform;
        //     if (camTrans.position != wpTransform.position ||  camTrans.rotation != wpTransform.rotation)
        //     {
        //        
        //         // float distance = Vector3.Distance(camTrans.position, wpTransform.position);
        //         // camTrans.position = Vector3.MoveTowards(camTrans.position, wpTransform.position,
        //         //     Time.deltaTime   );
        //         // camTrans.rotation = Quaternion.RotateTowards(camTrans.rotation , wpTransform.rotation, Time.deltaTime * _speedMoveCamera);
        //         
        //     }
        //     else
        //     {
        //         
        //     }
        // }
        // else
        // {
        //     State = GameState.Ingame;
        // }

    }

    public void AddWaypoint(Transform newWaypoint)
    {
        waypoints.Add(newWaypoint);
        newWaypoint.name = "Waypoint : " + waypoints.Count;
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
            RemoveAllObjectPhysical();
            CurrentLevel++;
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
    public void RemoveAllObjectPhysical()
    {
        _objectPhysicsList = new List<ObjectPhysics>();
    }
    
}
