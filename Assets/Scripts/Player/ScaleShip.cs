using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
public class ScaleShip : MonoBehaviour
{
    /// <summary>
    /// Scale of the ship
    /// </summary>
    private float _scaleFactor = 1;
    [SerializeField] private float heightOffset;
    [Header("Sound Aliases")]
    [SerializeField,Aliase] private string aliaseGrowing;

    public void SetScaleFactor(float h)
    {
        AudioManager.PlaySoundAtPosition(aliaseGrowing, transform.position);
        _scaleFactor = h;
    }
    public float GetScaleFactor()
    {
        return _scaleFactor;
    }

    private void Update()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            var transformPosition = transform.position;
            var heightFloor = LevelManager.Instance.GetCurrentFloor.transform.position.y;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * _scaleFactor, 1);
            Vector3 targetPos = new Vector3(transformPosition.x, (_scaleFactor/2) + (heightFloor + heightOffset), transformPosition.z);
            transform.position = Vector3.MoveTowards(transformPosition, targetPos, Time.deltaTime);
        }
      
    }


}
