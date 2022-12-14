using System;
using System.Collections;
using System.Collections.Generic;
using AudioAliase;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
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
                if(_instance == null)
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
    public int CurrentLevel {
        get
        {
            return _currentLevel;
        }
        private set
        {
            _currentLevel = value;
        } 
    }

    [SerializeField] [Aliase] private string onChangeLevelAlias;
    [SerializeField] [Aliase] private string musicBackground;
    private AudioPlayer _audioPlayerBgMusic;
    [SerializeField] 
    [Range(0.01f,2)]
    private float shipStartScale = 1;
    
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

    #region Action

    public Action CallbackLevelChange;
    /// <summary>
    /// Callback of methods when current level change
    /// </summary>
    public Action CallbackPreLevelChange;

    public Action CallbackEndgame;

    #endregion

    #region Properties

    public float ShipStartScale => shipStartScale;
    public GameState State { get; private set; }
    public List<ObjectPhysics> CurrentObjectList => _objectPhysicsList;
    
    public DataLevel CurrentDataLevel
    {
        get
        {
            if (CurrentLevel >= dataLevels.Length)
            {
                return dataLevels[^1];
            }

            return dataLevels[CurrentLevel];
        }
    }
    public GameObject GetCurrentFloor {
        get
        {
            if (CurrentDataLevel.floorCollision == null )
            {
                Debug.LogWarning($"No floor detected for the level {CurrentLevel}");
                return null;
            }
            return CurrentDataLevel.floorCollision;
        }
    }
    public Transform CurrentPlayerSpawnPoint => CurrentDataLevel.playerSpawnPoint;
    public float GetCurrentHeightOffset => CurrentDataLevel.heightOffset;
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

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        _instance = this;
        if (GameManager.Instance != null)
        {
            Debug.Log("LevelManager call GameManager");
        }
        // If dataLevels are not defined
        if (dataLevels.Length == 0)
        {
            dataLevels = new DataLevel[1];
            dataLevels[0].heightOffset = 5;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(DataLevels);
        Instance = this;
        _objectPhysicsList = new List<ObjectPhysics>();
        _camera ??= Camera.main;
        _smoothPath = GetComponent<CinemachineSmoothPath>();
        _player = FindObjectOfType<ShipController>();
        _player.GetComponent<ScaleShip>().SetScaleFactor(shipStartScale);
        _player.GetComponentInChildren<Absorber>().SetStrenght = shipStartScale;
        _player.transform.position = CurrentPlayerSpawnPoint.position;
        if (!_camera.TryGetComponent<CinemachineBrain>(out var _))
        {
            _camera.AddComponent<CinemachineBrain>();
        }
        if (_virtualCamera == null)
        {
            var cm = FindObjectOfType<CinemachineVirtualCamera>();
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
        CallbackLevelChange += ActivePlayableVolumeForCurrentLevel;
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
        AudioManager.PlayLoopSound(musicBackground,Vector3.zero, ref _audioPlayerBgMusic);
        
    }

  
    // Update is called once per frame
    private void Update()
    {
        if (GameManager.State == GameGlobalState.Ingame && State != GameState.Endgame)
        {
            // The camera need to go to the next postion 
            if (_dollyCart.m_Position < CurrentDataLevel.dollyCartPosition)
            {
                // Force the camera to look a next object when it is moving to next level
                if( CurrentPlayerSpawnPoint != null)
                    _virtualCamera.LookAt = GetLookAtPoint();
                else
                    _virtualCamera.LookAt = null;
                // Set speed of camera and go pass state to isMoving
                _dollyCart.m_Speed = CurrentDataLevel.cameraSpeed;
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
    }

    #endregion
    
    private Transform _lookAtTransform;
    

    Transform GetLookAtPoint()
    {
        _lookAtTransform.position = Vector3.Lerp(_lookAtTransform.position, CurrentPlayerSpawnPoint.position, Time.deltaTime);
        return _lookAtTransform;
    }
    private void ActivePlayableVolumeForCurrentLevel()
    {
        if (CurrentDataLevel.playableVolume == null)
        {
            Debug.LogWarning($"[LevelManager] No playableVolume assign for Level {CurrentLevel}, assign it.");
            return;
        }
        foreach (var dataLevel in dataLevels)
        {
            dataLevel.playableVolume.gameObject.SetActive(false);
        }
        CurrentDataLevel.playableVolume.gameObject.SetActive(true);
        CurrentDataLevel.playableVolume.Activate();
    }
    /// <summary>
    /// Active the indexed collision to build collision for environnement </summary>
    private void SetCollisionActive()
    {
        foreach (var dataLevel in dataLevels)
        {
            if(dataLevel.collision != null)   
                dataLevel.collision.gameObject.SetActive(false);
        }
        if (CurrentDataLevel.collision == null)
        {
           // Debug.LogWarning("[LevelManager] collision undefined");
            return;
        }
        CurrentDataLevel.collision.gameObject.SetActive(true);
    }
    /// <summary>
    /// Active the indexed floor to allow player movement
    /// </summary>
    private void SetFloorActive()
    {
        if (CurrentDataLevel.floorCollision == null)
        {
            Debug.LogWarning("[LevelManager] FloorCollision undefined");
            return;
        }
        foreach (var dataLevel in dataLevels)
        {
            dataLevel.floorCollision.gameObject.SetActive(false);
        }
        CurrentDataLevel.floorCollision.SetActive(true);
    }
    private void WatchObjectPhysicalAvailable()
    {
        foreach(var myObject in _objectPhysicsList)
        {
            // A object is available
            if (myObject != null)
            {
                return;
            }
        }
        RemoveAllObjectPhysical();
        CurrentLevel++;
        // Endgame 
        if (CurrentLevel >= dataLevels.Length)
        {
            State = GameState.Endgame;
            CallbackEndgame?.Invoke();
            return;
        }
        AudioManager.PlaySoundAtPosition(onChangeLevelAlias, Vector3.zero);
        
        // Check if the level is playable or not, otherwise we go directly on the next one
        while( CurrentDataLevel.skip)
        {
            CurrentLevel++;
        }
        // Call all methods registered in his events
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
    /// <summary>
    /// Clear the list of absordable objects and recreate it
    /// </summary>
    public void RemoveAllObjectPhysical()
    {
        _objectPhysicsList = new List<ObjectPhysics>();
    }

    int GetNumberOfObjectInLevel(int level)
    {
        var allObjectPhysics = FindObjectsOfType<ObjectPhysics>();
        int count = 0;
        for (int i = 0; i < allObjectPhysics.Length; i++)
        {
            if (allObjectPhysics[i].SleepUntilLevel == level)
                count++;
        }

        return count;
    }
    public ObjectPhysics[] GetObjectsInLevel(int level)
    {
        var allObjectPhysics = FindObjectsOfType<ObjectPhysics>();
        ObjectPhysics[] objectPhysics = new ObjectPhysics[GetNumberOfObjectInLevel(level)];
        int index = 0;
        for (int i = 0; i < allObjectPhysics.Length; i++)
        {
            if (allObjectPhysics[i].SleepUntilLevel == level)
            {
                objectPhysics[index] = allObjectPhysics[i];
                index++;
            }
                   
        }

        return objectPhysics;
    }

}
