using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Light))]
[RequireComponent(typeof(CapsuleCollider))]
public class Absorber : MonoBehaviour
{
    private Light _light;

    private CapsuleCollider _collider;
    [SerializeField] private float strenght = 35;
    [SerializeField] private float radius;

    [SerializeField] private List<GameObject> inTheTrigger;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponent<Light>();
        _collider = GetComponent<CapsuleCollider>();
        _collider.direction = 2;
    }

    // Update is called once per frame
    void Update()
    {
        _light.range = radius;
        _collider.height = radius * 2;
        Vector3 centerCollider = _collider.center;
        centerCollider.z = radius;
        _collider.center = centerCollider;
    }

    private void FixedUpdate()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            foreach (var gaby in inTheTrigger)
            {
                if (gaby.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    Vector3 direction = transform.position - gaby.transform.position;
                    rb.AddForce(direction*strenght);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!inTheTrigger.Contains(other.gameObject))
            inTheTrigger.Add(other.gameObject);
    }
}