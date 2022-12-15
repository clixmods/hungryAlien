using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameGlobalState
{
    Ingame,
    Paused
}
public enum UIType
{
    Menu,
    Ingame
}
public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if(_instance == null)
                    _instance = new GameObject("GameManager").AddComponent<GameManager>();
            }

            return _instance;
        }
        set => _instance = value;
    }
    
    #endregion
    
    [SerializeReference] private GameManagerData _data;

    public static event Action OnGameGlobalStateChanged;
    private GameGlobalState _state = GameGlobalState.Paused;
    private string _currentMap;
    public static GameGlobalState State
    {
        get
        {
            return Instance._state;
        }
        set
        {
            Instance._state = value;
            OnGameGlobalStateChanged?.Invoke();
        }
    }

    private void OnValidate()
    {
        transform.hideFlags = HideFlags.HideInInspector;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        _data = Resources.Load<GameManagerData>("GameManager Data");
       // QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        
       
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        _currentMap = SceneManager.GetActiveScene().name;
       //
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.name != _data.uiSceneName)
            _currentMap = arg0.name;
    }

    public void RestartMap()
    {
        SceneManager.LoadScene(_currentMap);
        State = GameGlobalState.Paused;
    }
    
    

    public static void CreateUI()
    {
        SceneManager.LoadScene(Instance._data.uiSceneName, LoadSceneMode.Additive);
    }

}
