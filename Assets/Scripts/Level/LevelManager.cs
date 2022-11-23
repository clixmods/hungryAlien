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

/// <summary>
/// Struct with multiple informations about levels in the LevelManager
/// </summary>
[System.Serializable]
public struct DataLevel
{
    
    [Tooltip("Name of the level")]
    public string name;
    [Header("Debug")]
    public bool skip;
    [Header("Camera")]
    [Prefix("[REQUIRED]")]
    [Tooltip("The position of the dolly Camera when the player is in his level")]
    public float dollyCartPosition ;
    [Tooltip("Speed of camera when it moves to the dollyCartPosition")]
    public float cameraSpeed;
    [Header("Gameplay")]
    [Prefix("[REQUIRED]")]
    [Tooltip("Collision boxs to indicate where the player can move")]
    public GameObject floorCollision;
    [Prefix("[REQUIRED]")]
    [Tooltip("The position of player spawn")]
    public Transform playerSpawnPoint;
    [Prefix("[REQUIRED]")]
    [Tooltip("A volume to secure position of ObjectPhysics")]
    public PlayableVolume playableVolume;
    [Tooltip("Collisions for objects when the level is active. Can be [Optional]")]
    public Collider collision;
    
    [Header("Player")]
    public float heightOffset;
    public float shipScaleAtTheEnd;
}

[RequireComponent(typeof(CinemachineSmoothPath))]
public class LevelManager : MonoBehaviour
{
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
    
   
    [field : SerializeField] public int CurrentLevel {
        get
        {
            return _currentLevel;
        }
        private set
        {
           
            
            _currentLevel = value;
            
        } 
    }
    [SerializeField] private DataLevel[] dataLevels;
    public DataLevel[] DataLevels => dataLevels;
    
    #region Private Variable
    private List<ObjectPhysics> _objectPhysicsList;
    private int _currentLevel;
    private CinemachineSmoothPath _smoothPath;
    private CinemachineDollyCart _dollyCart;
    private CinemachineVirtualCamera _virtualCamera;
    private ShipController _player;
    private Camera _camera;

    #endregion
    
    public Action CallbackLevelChange;
    /// <summary>
    /// Callback of methods when current level change
    /// </summary>
    public Action CallbackPreLevelChange;
    public GameState State { get; private set; }
    public List<ObjectPhysics> CurrentObjectList => _objectPhysicsList;
    public GameObject GetCurrentFloor {
        get
        {
            if (dataLevels[CurrentLevel].floorCollision == null )
            {
                Debug.LogWarning($"No floor detected for the level {CurrentLevel}");
                return null;
            }
            return dataLevels[CurrentLevel].floorCollision;
        }
    }
    public Transform CurrentPlayerSpawnPoint => dataLevels[CurrentLevel].playerSpawnPoint;
    public float GetCurrentHeightOffset => dataLevels[CurrentLevel].heightOffset;
  
    public ShipController Player 
    {
        get
        {
            if (_player == null)
            {
                _player = FindObjectOfType<ShipController>();
            }
            return _player;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _objectPhysicsList = new List<ObjectPhysics>();
        _camera ??= Camera.main;
        _smoothPath = GetComponent<CinemachineSmoothPath>();
        _player = GameObject.FindObjectOfType<ShipController>();
        if (!_camera.TryGetComponent<CinemachineBrain>(out var _))
        {
            _camera.AddComponent<CinemachineBrain>();
        }
        if (_virtualCamera == null)
        {
            var cm = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            if (cm == null)
            {
                var cmGameObject = Instantiate(new GameObject());
                _virtualCamera = cmGameObject.AddComponent<CinemachineVirtualCamera>();
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
        foreach (var dataLevel in DataLevels)
        {
            dataLevel.floorCollision.layer = LayerMask.NameToLayer("MoveZone");
        }
        CallbackLevelChange += SetFloorActive;
        CallbackLevelChange += SetActivePlayableVolume;
        CallbackLevelChange += SetCollisionActive;
        // Init Object physics
        var allObjectPhysics = FindObjectsOfType<ObjectPhysics>();
        foreach (var objPhysics in allObjectPhysics)
        {
            objPhysics.Init();
        }
        CallbackPreLevelChange();
        CallbackLevelChange();
        _lookAtTransform = new GameObject().transform;
        
        GameManager.CreateUI();
    }

    private Transform _lookAtTransform;
    Transform GetLookAtPoint()
    {
        _lookAtTransform.position = Vector3.Lerp(_lookAtTransform.position, CurrentPlayerSpawnPoint.position, Time.deltaTime);
        return _lookAtTransform;
    }
    // Update is called once per frame
    private void Update()
    {
        // The camera need to go to the next postion 
        if (_dollyCart.m_Position < dataLevels[CurrentLevel].dollyCartPosition)
        {
            // Force the camera to look a next object when it is moving to next level
            if( CurrentPlayerSpawnPoint != null)
                _virtualCamera.LookAt = GetLookAtPoint();
            else
                _virtualCamera.LookAt = null;
            // Set speed of camera and go pass state to isMoving
            _dollyCart.m_Speed = dataLevels[CurrentLevel].cameraSpeed;
            State = GameState.CameraIsMoving;
        }
        else
        {
            _virtualCamera.LookAt = GetLookAtPoint();
            _dollyCart.m_Speed = 0;
            State = GameState.Ingame;
            WatchObjectPhysicalAvailable();
        }
    }
    private void SetActivePlayableVolume()
    {
        if (dataLevels[CurrentLevel].playableVolume == null)
        {
            Debug.LogWarning($"[LevelManager] No playableVolume assign for Level {CurrentLevel}, assign it.");
            return;
        }
        foreach (var dataLevel in dataLevels)
        {
            dataLevel.playableVolume.gameObject.SetActive(false);
        }
        dataLevels[CurrentLevel].playableVolume.gameObject.SetActive(true);
        dataLevels[CurrentLevel].playableVolume.Activate();
    }
    /// <summary>
    /// Active the indexed collision to build collision for environnement </summary>
    void SetCollisionActive()
    {
        foreach (var dataLevel in dataLevels)
        {
            if(dataLevel.collision != null)   
                dataLevel.collision.gameObject.SetActive(false);
        }
        if (dataLevels[CurrentLevel].collision == null)
        {
           // Debug.LogWarning("[LevelManager] collision undefined");
            return;
        }
        dataLevels[CurrentLevel].collision.gameObject.SetActive(true);
    }
    /// <summary>
    /// Active the indexed floor to allow player movement
    /// </summary>
    void SetFloorActive()
    {
        if (dataLevels[CurrentLevel].floorCollision == null)
        {
            Debug.LogWarning("[LevelManager] FloorCollision undefined");
            return;
        }
        foreach (var dataLevel in dataLevels)
        {
            dataLevel.floorCollision.gameObject.SetActive(false);
        }
        dataLevels[CurrentLevel].floorCollision.SetActive(true);
    }
    private void WatchObjectPhysicalAvailable()
    {
        //bool everythingIsAbsorbed = true;
        foreach(var myObject in _objectPhysicsList)
        {
            if (myObject != null)
            {
                //everythingIsAbsorbed = false;
                return;
            }
        }
        //if (!everythingIsAbsorbed) 
        RemoveAllObjectPhysical();
        CurrentLevel++;
        while( dataLevels[CurrentLevel].skip)
        {
            CurrentLevel++;
        }
        CallbackPreLevelChange();
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
