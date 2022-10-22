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
    [SerializeField] Transform AbsorbePoint;
    [SerializeField] ScaleShip scaleShip;
    [SerializeField] float scaleMultiplier = 1.1f;
    [SerializeField] float strengthMultiplier = 1.1f;

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
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            if (other.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 direction = AbsorbePoint.position - other.transform.position;
                rb.AddForce(direction * strenght);
                rb.velocity = rb.velocity.normalized * Mathf.Clamp(rb.velocity.magnitude, 0, 5);
                if (AbsorbePoint.position.y - other.transform.position.y < 1f)
                {
                    Destroy(other.gameObject);
                    scaleShip.SetScaleFactor(scaleShip.GetScaleFactor() * scaleMultiplier);
                    strenght *= strengthMultiplier;
                }
            }
        }
    }
}