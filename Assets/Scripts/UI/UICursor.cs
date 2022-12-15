using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    private Camera _camera;
    private InputAsset _input;
    private Vector3 _offset;
    private Image _image;
    [SerializeField] private Color cursorColor;

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

    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        GameManager.OnGameGlobalStateChanged += SetCursorVisibility;
        AbsorberColor.CallbackColorChange += SetColor;
    }

    private void OnDestroy()
    {
        GameManager.OnGameGlobalStateChanged -= SetCursorVisibility;
        AbsorberColor.CallbackColorChange -= SetColor;
    }

    void SetColor(Color color)
    {
        cursorColor = color;
    }

    private const float ChangeColorMultiplier = 10f;
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
        // Change cursor color
        if (_image.color != cursorColor)
        {
            _image.color = Color.Lerp(_image.color, cursorColor, Time.deltaTime * ChangeColorMultiplier);
        }
        
        transform.position =  point + _offset;
       
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

        if (LevelManager.Instance.State == GameState.Endgame)
        {
            gameObject.SetActive(false);
            Cursor.visible = true;
        }
    }
}
