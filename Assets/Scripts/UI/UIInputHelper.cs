using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class UIInputHelper : MonoBehaviour , IUIWaypoint
{
    private float _lifeTime = -1;
    private Camera _camera;
    private Vector3 _offset;
    private Transform _targetTransform;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    private Image _background;
    private float _speedOpacity = 2;
    private Color _textMeshProUGUIInitialColor;
    private Color _backgroundInitialColor;
    public string Text {
        get { return textMeshProUGUI.text ; }
        set { textMeshProUGUI.text = value; } 
    }
    public Transform TargetTransform
    {
        get => _targetTransform;
        set => _targetTransform = value;
    }
    
    public void Setup(string text, Transform transformToTarget , float displayTime = -1)
    {
        Text = text;
        _targetTransform = transformToTarget;
        _lifeTime = displayTime;
    }
    
    
    // Start is called before the first frame update
    void Awake()
    {
        textMeshProUGUI ??= GetComponentInChildren<TextMeshProUGUI>();
        _background = GetComponentInChildren<Image>();
        _textMeshProUGUIInitialColor = textMeshProUGUI.color;
        _backgroundInitialColor = _background.color;
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        if (_lifeTime != -1)
        {
            if (_lifeTime > 0)
            {
                _lifeTime -= Time.deltaTime;
            }
            else
            {
                // textMeshProUGUI.color = Color.Lerp(textMeshProUGUI.color, Color.clear, Time.deltaTime);
                // _background.color = Color.Lerp(_background.color, Color.clear, Time.deltaTime);
                // if(textMeshProUGUI.color == Color.clear &&  _background.color == Color.clear)
                //     Destroy(gameObject);
            }
        }
        
    }
    
    void UpdatePosition()
    {
        float progressValue = Time.deltaTime * _speedOpacity;
        // Si l'object a follow est detruit, on le delete
        if(true && _targetTransform == null)
        {
            transform.position = new Vector3(-10000,0,-100);
            textMeshProUGUI.color = Color.Lerp(textMeshProUGUI.color, Color.clear, progressValue);
            _background.color = Color.Lerp(_background.color, Color.clear, progressValue);
            return;
        }
        Vector3 position = _camera.WorldToScreenPoint(_targetTransform.position);
  
        // Permet de voir si l'object est derriere la camera
        bool isBehindTheCamera = position.x < 0 ||position.y < 0 || position.z < 0;
        if(isBehindTheCamera )
        {
            transform.position = new Vector3(-10000,0,-100);
            textMeshProUGUI.color = Color.Lerp(textMeshProUGUI.color, Color.clear, progressValue);
            _background.color = Color.Lerp(_background.color, Color.clear, progressValue);
        }
        else
        {
            textMeshProUGUI.color = Color.Lerp(textMeshProUGUI.color, _textMeshProUGUIInitialColor,progressValue);
            _background.color = Color.Lerp(_background.color, _backgroundInitialColor, progressValue);
            // if(hintPro.offset == null)
            transform.position = position + _offset;
            // else
            //     hintstring.transform.position = position + hintPro.offset;
        }
    }

    public void HideHelper()
    {
        _targetTransform = null;
    }
    public void DrawContent()
    {
        throw new System.NotImplementedException();
    }
}
