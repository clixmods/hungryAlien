using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
public class ScaleShip : MonoBehaviour
{
    /// <summary>
    /// Scale of the ship
    /// </summary>
    float _scaleFactor = 1;
    [SerializeField] private float speedHeightMove = 5;
    [SerializeField] private float heightOffset;
    
    [Header("Sound Aliases")]
    [SerializeField,Aliase] private string aliaseGrowing;

    private float _heightFloor;
    public void SetScaleFactor(float h)
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
    }

    private void Update()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            var transformPosition = transform.position;
            var heightFloor = _heightFloor;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * _scaleFactor, 1);
            Vector3 targetPos = new Vector3(transformPosition.x, (_scaleFactor/2) + (heightFloor + heightOffset + _scaleFactor), transformPosition.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speedHeightMove);
        }
        
      
    }

    void ChangeHeightFloor()
    {
        GameObject currentFloor = LevelManager.Instance.GetCurrentFloor;
        if(currentFloor != null)
            _heightFloor = currentFloor.transform.position.y;
    }

}
