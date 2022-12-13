using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
using Level;

public enum ShipState
{
    Idle,
    OnObject
}
public class ScaleShip : MonoBehaviour
{
    /// <summary>
    /// Scale of the ship
    /// </summary>
    float _scaleFactor = 1;
    [SerializeField] private float speedHeightMove = 5;
    private ShipController _shipController;
    [Header("Sound Aliases")]
    [SerializeField,Aliase] private string aliaseGrowing;

    private float _heightFloor;
    public void SetScaleFactor(float h)
    {
        _scaleFactor = h;
    }
    public void AddScaleFactor(float h)
    {
        AudioManager.PlaySoundAtPosition(aliaseGrowing, transform.position);
        _scaleFactor += h;
    }
    public float GetScaleFactor()
    {
        return _scaleFactor;
    }

    private void Start()
    {
        LevelManager.Instance.CallbackPreLevelChange += ChangeHeightFloor;
        _shipController = LevelManager.Instance.Player;
    }

    private void Update()
    {
        switch (_shipController.State)
        { 
            case ShipState.OnObject:
            case ShipState.Idle:
                if (LevelManager.Instance.State == GameState.Ingame)
                {
                    var transformPosition = transform.position;
                    ChangeHeightFloor();
                    var heightFloor = _heightFloor;
                    var heightOffset = LevelManager.Instance.GetCurrentHeightOffset;
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * _scaleFactor, 1);
                    float heightObject = GetHeightObject();
                    
                        
                    // BUG : difference calculation between editor play and build
                    Vector3 targetPos = new Vector3(transformPosition.x, heightObject+ heightFloor + heightOffset + _scaleFactor, transformPosition.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * (speedHeightMove + _scaleFactor));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    float GetHeightObject()
    {
        float greater = 0;
        if (_shipController.Absorber.InTheTrigger != null && _shipController.Absorber.InTheTrigger.Count > 0)
        {
            if (_shipController.Absorber.InTheTrigger[0].TryGetComponent<IAbsorbable>(out var objectPhysics))
            {
                greater = objectPhysics.HeightObject;
                for (int i = 0; i < _shipController.Absorber.InTheTrigger.Count; i++)
                {
                    if (_shipController.Absorber.InTheTrigger[i].TryGetComponent<IAbsorbable>(out var aobjectPhysics))
                    {
                        float heightToCheck = aobjectPhysics.HeightObject;
                        if (heightToCheck > greater)
                        {
                            greater = heightToCheck;
                        }
                    }
                }
            }
            
        }
        return greater/2;
    }

    void ChangeHeightFloor()
    {
        _heightFloor = 0;
        GameObject currentFloor = LevelManager.Instance.GetCurrentFloor;
        if(currentFloor != null)
            _heightFloor += currentFloor.transform.position.y;
    }

}
