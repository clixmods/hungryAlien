using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHelperManager : MonoBehaviour
{
    private ShipController _shipController;
    private Absorber _absorber;

    private bool _hasPressAbsorbed;
    private UIInputHelper _uiHelperAbsorbed;
    [SerializeField] private string pressAbsorbedText = "Press Left Click to absorb";
    
    private bool _hasPressMovement;
    private UIInputHelper _uiHelperMovement;
    [SerializeField] private string pressMovementText = "Use mouse to move";
    private float _displayTime;
  
    // Start is called before the first frame update
    void Start()
    {
        _shipController = GetComponentInChildren<ShipController>();
        _absorber = GetComponentInChildren<Absorber>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_hasPressMovement && _uiHelperMovement == null)
        {
            //UIManager.CreateInputHelper(pressMovementText,transform, out _uiHelperMovement , 5);
            _hasPressMovement = true;
        }


        if (_uiHelperAbsorbed != null && _uiHelperAbsorbed.TargetTransform != null && !_absorber.InTheTrigger.Contains( _uiHelperAbsorbed.TargetTransform.gameObject))
        {
            _uiHelperAbsorbed.HideHelper();
        }
        if (_absorber.InTheTrigger != null && _absorber.InTheTrigger.Count != 0 && !_hasPressAbsorbed )
        {
            var targetTransform = _absorber.InTheTrigger[0].transform;
            if (_uiHelperAbsorbed != null)
            {
                _uiHelperAbsorbed.TargetTransform = targetTransform;
            }
            else
            {
                UIManager.CreateInputHelper(pressAbsorbedText,targetTransform, out _uiHelperAbsorbed);
            }
        }
        
        if (Mouse.current.leftButton.isPressed && _uiHelperAbsorbed != null)
        {
            _hasPressAbsorbed = true;
            _uiHelperAbsorbed.HideHelper();
        }
    }
}
