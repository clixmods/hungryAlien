using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;


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
    public static void CreateUI()
    {
        SceneManager.LoadScene(Instance._data.uiSceneName);
        // switch (uiType)
        // {
        //     case UIType.Menu:
        //         throw new ArgumentOutOfRangeException(nameof(uiType), uiType, null);
        //         break;
        //     case UIType.Ingame:
        //         throw new ArgumentOutOfRangeException(nameof(uiType), uiType, null);
        //         break;
        //     default:
        //         throw new ArgumentOutOfRangeException(nameof(uiType), uiType, null);
        // }
    }

}
