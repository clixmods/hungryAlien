using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UICursor : MonoBehaviour
{
    private Camera _camera;
    private InputAsset _input;
    private Vector3 _offset;

    /// <summary>
    /// Input used by the player
    /// </summary>
    public InputAsset Input
    {
        get
        {
            // Prevent null ref when the game reload script
            if (_input == null)
            {
                _input = new InputAsset();
            }
            return _input;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        GameManager.OnGameGlobalStateChanged += SetCursorVisibility;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_camera == null)
        {
            _camera = Camera.main;
            return;
        }

        Vector3 point = Mouse.current.position.ReadValue();   
        point.z = _camera.nearClipPlane;
        Vector3 position = _camera.ScreenToWorldPoint(point);
     
        // Permet de voir si l'object est derriere la camera
        // bool isBehindTheCamera = position.x < 0 ||position.y < 0;
        // if(!isBehindTheCamera )
        // {
            // if(hintPro.offset == null)
            transform.position =  point + _offset;
            // else
            //     hintstring.transform.position = position + hintPro.offset;
        // }
    }

    private void SetCursorVisibility()
    {
        switch (GameManager.State)
        {
            case GameGlobalState.Ingame:
                gameObject.SetActive(true);
                Cursor.visible = false;
                break;
            case GameGlobalState.Paused:
                gameObject.SetActive(false);
                Cursor.visible = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
