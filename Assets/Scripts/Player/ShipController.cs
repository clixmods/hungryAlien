using DG.Tweening;
using UnityEngine;
using AudioAliase;

[RequireComponent(typeof(CameraShake))]
public class ShipController : MonoBehaviour
{
    private InputAsset _input;
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
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float speed = 5;
    
    [Header("Sound Aliases")]
    [SerializeField, Aliase] private string aliaseIdle;
    [SerializeField, Aliase] private string aliaseMoving;
    [SerializeField, Aliase] private string aliaseUpToSky;

    private Camera _camera;
    /// <summary>
    /// The last valid point for player movement
    /// </summary>
    private Vector3 _lastHitPoint;
    /// <summary>
    /// ref for the audioplayer used when the player move
    /// </summary>
    private AudioPlayer _audioMoving;
    private AudioPlayer _audioIdle;
    
    public Absorber Absorber { get; private set; }
    
    private ShipState _state;
    public ShipState State
    {
        get => _state;
        set => _state = value;
    }
    
    #region MonoBehaviour
    private void Awake()
    {
        _camera = Camera.main;
        Absorber = GetComponentInChildren<Absorber>();
    }

    private void OnEnable()
    {
        Input.Enable();
    }
    private void OnDisable()
    {
        Input.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
        Input.Game.Pause.performed += ctx => UIManager.Instance.PauseGame();
    }
    void FixedUpdate()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            FollowCursor();
        }
        if (LevelManager.Instance.State == GameState.CameraIsMoving)
        {
            Vector3 nextPlayerSpawnPoint = LevelManager.Instance.CurrentPlayerSpawnPoint.position;
            if (transform.position.y < (nextPlayerSpawnPoint.y + 10 + transform.localScale.magnitude)  )
            {
                Vector3 direction = new Vector3( nextPlayerSpawnPoint.x-transform.position.x, 5, nextPlayerSpawnPoint.z-transform.position.z);
                transform.position += direction * speed * Time.deltaTime ;
            }
            _lastHitPoint = transform.position;
        }
    }
    #endregion
    private void FollowCursor()
    {
        var mousePosition = MouseToWorldPosition();
        var shipPosition = transform.position;
        var direction = new Vector3((mousePosition.x - shipPosition.x), 0, (mousePosition.z - shipPosition.z));
        
        if(direction.magnitude >0.2f)
        {
            transform.position += direction * speed * Time.deltaTime ;
        }
        else
        {
            AudioManager.PlayLoopSound(aliaseIdle, transform, ref _audioIdle);
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
        ray = _camera.ScreenPointToRay(Input.Game.Cursor.ReadValue<Vector2>());
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