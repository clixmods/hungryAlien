using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipViewModel : MonoBehaviour
{
    [SerializeField] private float speedRotation = 5;
    [SerializeField] private GameObject viewModel;
     // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var euler = viewModel.transform.localRotation.eulerAngles;
        euler.y += Time.deltaTime * speedRotation * 100;
        viewModel.transform.localRotation = Quaternion.Euler(euler);
    }
}
