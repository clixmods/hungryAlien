using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleShip : MonoBehaviour
{
    float scaleFactor = 1;
    [SerializeField] float heightOffset;
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
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scaleFactor, 1);
        transform.position = new Vector3(transform.position.x, scaleFactor + heightOffset, transform.position.z);
    }


}
