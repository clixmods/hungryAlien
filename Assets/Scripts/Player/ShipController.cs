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
   
   
    [SerializeField] private InputAsset input;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float speed = 5;
    
    [Header("Sound Aliases")]
    [SerializeField, Aliase] private string aliaseIdle;
    [SerializeField, Aliase] private string aliaseMoving;
    [SerializeField, Aliase] private string aliaseUpToSky;

    private Camera _camera;
    private Vector3 _lastHitPoint;
    /// <summary>
    /// ref for the audioplayer used when the player move
    /// </summary>
    private AudioPlayer _audioMoving;
    private AudioPlayer _audioIdle;

    #region MonoBehaviour

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

    #endregion

    public void FollowCursor()
    {
        var transformPosition = transform.position;
        var mousePosition = MouseToWorldPosition();
        var direction = new Vector3((mousePosition.x - transformPosition.x), 0,
            (mousePosition.z - transformPosition.z));
        if (direction.magnitude > 0.2f)
        {
            transform.position += direction * speed * Time.deltaTime;
            AudioManager.PlayLoopSound(aliaseMoving, transform, ref _audioMoving);
            AudioManager.StopLoopSound(ref _audioIdle);
        }
        else
        {
            AudioManager.PlayLoopSound(aliaseIdle, transform, ref _audioIdle);
            AudioManager.StopLoopSound(ref _audioMoving);
        }

        transform.DORotate(new Vector3(direction.z, 0, -direction.x), 0.1f);
    }

    /// <summary> Retourne la position de la souris dans le monde 3D en fonction du hit du raycast</summary> 
    private Vector3 MouseToWorldPosition()
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
            _lastHitPoint = Hitpoint;
            return Hitpoint;
        }

        return _lastHitPoint;
    }
}