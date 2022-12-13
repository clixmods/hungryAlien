using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DecalProjectorFixRotation : MonoBehaviour
{
    private Quaternion rotationToKeep = Quaternion.Euler(90,0,0);
    // Start is called before the first frame update
    void Start()
    {
        //rotationToKeep = Quaternion.Euler(90,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = rotationToKeep;
    }
}
