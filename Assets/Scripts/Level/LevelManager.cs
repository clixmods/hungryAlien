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

[System.Serializable]
public struct DataLevel
{
    public float dollyCartPosition ;
    public float cameraSpeed;
    public GameObject floorCollision;
    public Transform playerSpawnPoint;
}
[RequireComponent(typeof(CinemachineSmoothPath))]
public class LevelManager : MonoBehaviour
{
    [SerializeField] private float _speedMoveCamera = 10;
    
    #region Singleton

    private static LevelManager _instance;
    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
                if(!_instance)
                    _instance = new GameObject("LevelManager").AddComponent<LevelManager>();
            }

            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    #endregion

    [field : SerializeField] public int CurrentLevel { get; private set; }
    public GameState State { get; private set; }
    public List<Transform> waypoints; // TODO : OBsolete ?
    
    /// <summary>
    /// List of GameObject where the ship can flew in the top
    /// </summary>
    [SerializeField,Tooltip("Place all GameObject where the ship can flew on top")] 
    private List<GameObject> floorCollision;

    [SerializeField] private DataLevel[] dataLevels;
    
    #region Private Variable

    private CinemachineSmoothPath _smoothPath;
    private CinemachineDollyCart _dollyCart;
    private CinemachineVirtualCamera _virtualCamera;
    private ShipController _player;
    private Camera _camera;

    #endregion
    
    /// <summary>
    /// Callback of methods when current level change
    /// </summary>
    public Action CallbackLevelChange;
    
 
    public int MaxLevel
    {
        get { return waypoints.Count; }
    }
    private List<ObjectPhysics> _objectPhysicsList;
    public GameObject GetCurrentFloor {
        get
        {
            if (floorCollision == null || floorCollision.Count == 0)
            {
                Debug.LogWarning($"No floor detected for the level {CurrentLevel}");
                return null;
            }
            return floorCollision[CurrentLevel];
        }
    } 
    
    public Transform CurrentPlayerSpawnPoint => dataLevels[CurrentLevel].playerSpawnPoint;
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
        CallbackLevelChange();
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (_dollyCart.m_Position <= CurrentLevel)

        if (_dollyCart.m_Position <= dataLevels[CurrentLevel].dollyCartPosition)
        {
            // Force the camera to look a next object when it is moving to next level
            if( _objectPhysicsList.Count > 0 && _objectPhysicsList[0] != null)
                _virtualCamera.LookAt = _objectPhysicsList[0].transform;
            else
            {
                _virtualCamera.LookAt = null;
            }
            
            _dollyCart.m_Speed = dataLevels[CurrentLevel].cameraSpeed;
            State = GameState.CameraIsMoving;
        }
        else
        {
            _virtualCamera.LookAt = _player.transform;
            _dollyCart.m_Speed = 0;
            State = GameState.Ingame;
            SetFloorActive(CurrentLevel);
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
    /// <summary>
    /// Active the indexed floor to allow player movement
    /// </summary>
    /// <param name="indexToActive"></param>
    void SetFloorActive(int indexToActive)
    {
        if (floorCollision == null || floorCollision.Count == 0)
        {
            Debug.LogWarning("[LevelManager] FloorCollision undefined");
            return;
        }
        
        foreach (var floorTransform in floorCollision)
        {
            floorTransform.gameObject.SetActive(false);
        }
        
        floorCollision[CurrentLevel].gameObject.SetActive(true);
        
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

        if (!rip) return;
        RemoveAllObjectPhysical();
        CurrentLevel++;
        CallbackLevelChange();
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
