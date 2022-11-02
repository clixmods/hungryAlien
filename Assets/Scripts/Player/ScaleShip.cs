using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
public class ScaleShip : MonoBehaviour
{
    float scaleFactor = 1;
    [SerializeField] private float speedHeightMove = 5;
    [SerializeField] float heightOffset;
    [Header("Sound Aliases")]
    [Aliase] public string aliaseGrowing;

    private float _heightFloor;
    public void SetScaleFactor(float h)
    {
        AudioManager.PlaySoundAtPosition(aliaseGrowing, transform.position);
        scaleFactor += h;
    }
    public float GetScaleFactor()
    {
        return scaleFactor;
    }

    private void Start()
    {
        LevelManager.Instance.CallbackLevelChange += ChangeHeightFloor;
    }

    private void Update()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            //float heightFloor = LevelManager.Instance.GetCurrentFloor.transform.position.y;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scaleFactor, 1);
            Vector3 targetPos = new Vector3(transform.position.x, (scaleFactor/2) + (_heightFloor + heightOffset), transform.position.z);

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
