using System;
using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FourcheEnigme : MonoBehaviour
{
    [SerializeField] GameObject[] _books;
    private Rigidbody[] _rbs;
    [SerializeField] private ObjectPhysicsNotAbsorbable objectToDestroy;
    private void Awake()
    {
        if (objectToDestroy == null)
        {
            Debug.LogError("Warning: objectToDestroy not defined");
        }
        _rbs = new Rigidbody[_books.Length];
        for (int i = 0; i < _books.Length; i++)
        {
            _rbs[i] = _books[i].GetComponentInChildren<Rigidbody>();
            _books[i].gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == objectToDestroy.gameObject)
        {
            other.gameObject.SetActive(false);
            for (int i = 0; i < _books.Length; i++)
            {
                _books[i].gameObject.SetActive(true);
                _books[i].GetComponent<IAbsorbable>().InitialPosition = _books[i].transform.position;
                _books[i].GetComponent<IAbsorbable>().WakeObject();
                _books[i].transform.position = other.gameObject.transform.position;
                var randomX = Random.Range(-5, 5);
                var randomY = Random.Range(-5, 5);
                var randomZ = Random.Range(-5, 5);
                var force = new Vector3(randomX, randomY, randomZ);
                _rbs[i].AddForce(force.normalized * Random.Range(0, 5), ForceMode.Impulse);
                
            }
            
        }
    }
}
