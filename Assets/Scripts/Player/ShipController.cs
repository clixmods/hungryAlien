using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;
using AudioAliase;

[RequireComponent(typeof(CameraShake))]
public class ShipController : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] InputAsset input;
    [SerializeField] LayerMask layerMask;

    [SerializeField] float speed = 5;
    Vector3 lastHitPoint;
    
    [Header("Sound Aliases")]
    [Aliase] public string aliaseIdle;
    [Aliase] public string aliaseMoving;
    [Aliase] public string aliaseUpToSky;

    private void Awake()
    {
        input = new InputAsset();
        _camera = Camera.main;
    }
    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            FollowCursor();
        }
        
        if (LevelManager.Instance.State == GameState.CameraIsMoving)
        {
            Vector3 direction = new Vector3(0, 5, 0);
            transform.position += direction * speed * Time.deltaTime;
        }
        
    }
    
    public void FollowCursor()
    {
        Vector3 direction = new Vector3((MouseToWorldPosition().x - transform.position.x), 0, (MouseToWorldPosition().z - transform.position.z));
//        Debug.Log(direction.magnitude);
        if(direction.magnitude >0.2f)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
        
        transform.DORotate(new Vector3(direction.z, 0, -direction.x), 0.1f);
    }
        /// <summary> Retourne la position de la souris dans le monde 3D en fonction du hit du raycast</summary> 
    Vector3 MouseToWorldPosition()
    {
        RaycastHit RayHit;
        Ray ray;
        Vector3 Hitpoint = Vector3.zero;
        // On trace un rayon avec la mousePosition de la souris
        ray = _camera.ScreenPointToRay(input.Game.Cursor.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out RayHit, Mathf.Infinity, layerMask))
        {
            Hitpoint = new Vector3(RayHit.point.x, RayHit.point.y, RayHit.point.z);

            if (Hitpoint != null)
                Debug.DrawLine(_camera.transform.position, Hitpoint, Color.blue, 0.5f);
            lastHitPoint = Hitpoint;
            return Hitpoint;
        }
        else
        {
            
            return lastHitPoint;
        }

        
    }
}

