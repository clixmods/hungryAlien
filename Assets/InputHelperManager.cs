using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHelperManager : MonoBehaviour
{
    private ShipController _shipController;
    private Absorber _absorber;

    private bool _hasPressAbsorbed;
    private UIInputHelper _uiHelperAbsorbed;

    private bool _hasPressMovement;
    private UIInputHelper _uiHelperMovement;

    [SerializeField] private string pressMovementText = "Use mouse to move";
    [SerializeField] private string pressAbsorbedText = "Press Left Click to absorb";

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
            UIManager.CreateInputHelper(pressMovementText,transform, out _uiHelperMovement);
        }

        if (_uiHelperAbsorbed != null && !_absorber.InTheTrigger.Contains(_uiHelperAbsorbed.TargetTransform.gameObject))
        {
            Destroy(_uiHelperAbsorbed.gameObject);
        }
            
        
        
        if (_absorber.InTheTrigger != null && _absorber.InTheTrigger.Count != 0 && !_hasPressAbsorbed && _uiHelperAbsorbed == null)
        {
            UIManager.CreateInputHelper(pressAbsorbedText,_absorber.InTheTrigger[0].transform, out _uiHelperAbsorbed);
        }

        if (Mouse.current.leftButton.isPressed && _uiHelperAbsorbed != null)
        {
            _hasPressAbsorbed = true;
            Destroy(_uiHelperAbsorbed.gameObject);
        }
    }
}
