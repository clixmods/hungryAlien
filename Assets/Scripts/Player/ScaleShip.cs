using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;
public class ScaleShip : MonoBehaviour
{
    float scaleFactor = 1;
    [SerializeField] float heightOffset;
    [Header("Sound Aliases")]
    [Aliase] public string aliaseGrowing;

    public void SetScaleFactor(float h)
    {
        scaleFactor = h;
    }
    public float GetScaleFactor()
    {
        return scaleFactor;
    }

    private void Update()
    {
        if (LevelManager.Instance.State == GameState.Ingame)
        {
            float heightFloor = LevelManager.Instance.GetCurrentFloor.transform.position.y;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scaleFactor, 1);
            Vector3 targetPos = new Vector3(transform.position.x, (scaleFactor/2) + (heightFloor + heightOffset), transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime);
        }
      
    }


}
