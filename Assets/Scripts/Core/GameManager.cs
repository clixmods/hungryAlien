using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using JetBrains.Annotations;
using UnityEngine;


public enum UIType
{
    Menu,
    Ingame
}
public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager _instance;
    private static GameManager Instance
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

    private GameManagerData _data;
    

    public static void CreateUI(UIType uiType)
    {
        switch (uiType)
        {
            case UIType.Menu:
                break;
            case UIType.Ingame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(uiType), uiType, null);
        }
    }

}
