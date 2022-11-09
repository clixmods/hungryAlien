using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(TextMeshProUGUI))]
public class UIInputHelper : MonoBehaviour , IUIWaypoint
{

    private Camera _camera;
    private Vector3 _offset;
    private Transform _targetTransform;
    private TextMeshProUGUI _textMeshPro;
    public string Text {
        get { return _textMeshPro.text ; }
        set { _textMeshPro.text = value; } 
    }

    public Transform TargetTransform
    {
        get { return _targetTransform; }
    }

    
    public void Setup(string text, Transform transformToTarget)
    {
        Text = text;
        _targetTransform = transformToTarget;
    }
    
    
    // Start is called before the first frame update
    void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }
    
    void UpdatePosition()
    {
        // Si l'object a follow est detruit, on le delete
        if(true && _targetTransform == null)
        {
            Destroy(gameObject);
        }
        // if (_targetTransform == null || !hintPro.FollowRelatedObject)
        // {
        //     return;
        // }
               

        Vector3 position = _camera.WorldToScreenPoint(_targetTransform.position);
        // Permet de voir si l'object est derriere la camera
        bool isBehindTheCamera = position.x < 0 ||position.y < 0 || position.z < 0;
        if(!isBehindTheCamera )
        {
                // if(hintPro.offset == null)
                    transform.position = position + _offset;
                // else
                //     hintstring.transform.position = position + hintPro.offset;
        }
        else
        {
            transform.position = new Vector3(-10000,0,-100);
        }
    }
    public void DrawContent()
    {
        throw new System.NotImplementedException();
    }
}
