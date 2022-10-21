using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleShip : MonoBehaviour
{
    [Range(1f, 10f)]
    [SerializeField] float scaleFactor;
    [SerializeField] float heightOffset;

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scaleFactor, 1);
        transform.position = new Vector3(transform.position.x, scaleFactor + heightOffset, transform.position.z);
    }


}
