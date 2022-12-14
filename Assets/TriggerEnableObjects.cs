using System;
using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = System.Object;
using Random = UnityEngine.Random;

public class TriggerEnableObjects : MonoBehaviour
{
    [FormerlySerializedAs("_books")] [SerializeField] GameObject[] objectToEnables;
    private Rigidbody[] _rbs;
    [SerializeField] private ObjectPhysicsNotAbsorbable objectToDestroy;
    [SerializeField] private bool destroyObjectToDestroy = true;
    [SerializeField] private bool pushObjectOnTriggered = true;
    [SerializeField] private bool triggerObjectIsDissolvable = false;

    private bool _isTriggered;
    private List<MaterialPropertyBlockManager> _propBlockManagers;
    private MeshRenderer[] _meshRenderers;
    [SerializeField] private bool ObjectAreDisabled;
    private Collider _collider;

    public bool pushObject;
    private void Awake()
    {
        if (objectToDestroy == null)
        {
            Debug.LogError("Warning: objectToDestroy not defined");
        }
        _rbs = new Rigidbody[objectToEnables.Length];
        for (int i = 0; i < objectToEnables.Length; i++)
        {
            _rbs[i] = objectToEnables[i].GetComponentInChildren<Rigidbody>();
         
        }

        _collider = GetComponent<Collider>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        // Generate MaterialPropertyBlock
        _propBlockManagers = new List<MaterialPropertyBlockManager>();
        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            var mtlManager = gameObject.AddComponent<MaterialPropertyBlockManager>();
            mtlManager.Init(_meshRenderers[i]);
            _propBlockManagers.Add(mtlManager);
        }
    }

    private void Start()
    {
      
    }

    private void Update()
    {
        if (pushObject)
        {
            for (int i = 0; i < objectToEnables.Length; i++)
            {
                objectToEnables[i].gameObject.SetActive(true);
                objectToEnables[i].GetComponent<IAbsorbable>().InitialPosition = objectToEnables[i].transform.position;
                if (pushObject)
                {
                   
                    var randomX = Random.Range(-5, 5);
                    var randomY = Random.Range(-5, 5);
                    var randomZ = Random.Range(-5, 5);
                    var force = new Vector3(randomX, 5, randomZ);
                    _rbs[i].AddForce( force * Random.Range(100, 200)* Time.deltaTime, ForceMode.Impulse);
                }
            }
            pushObject = false;
        }
        if (!ObjectAreDisabled)
        {
            // ca fait buguer fdp TODO
            for (int i = 0; i < objectToEnables.Length; i++)
            {
                objectToEnables[i].gameObject.SetActive(false);
            }

            ObjectAreDisabled = true;
        }
        if (triggerObjectIsDissolvable && _isTriggered)
        {
            for (int i = 0; i < _propBlockManagers.Count; i++)
            {
                _propBlockManagers[i].AddDissolve(Time.deltaTime);
            }
            // Destrution of Objects
            bool destroyIt = true;
            for (int index = 0; index < _propBlockManagers.Count; index++)
            {
                if(_propBlockManagers[index].FloatsIsLessThan(1))
                {
                    destroyIt = false;
                }
            }
            if (destroyIt)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == objectToDestroy.gameObject)
        {
            if (destroyObjectToDestroy)
            {
                other.gameObject.SetActive(false);
            }

            _collider.enabled = false;
            _isTriggered = true;
           
            for (int i = 0; i < objectToEnables.Length; i++)
            {
                objectToEnables[i].gameObject.SetActive(true);
                objectToEnables[i].GetComponent<IAbsorbable>().InitialPosition = objectToEnables[i].transform.position;
                if (pushObjectOnTriggered)
                {
                    objectToEnables[i].transform.position = other.gameObject.transform.position;
                    var randomX = Random.Range(-5, 5);
                    var randomY = Random.Range(-5, 5);
                    var randomZ = Random.Range(-5, 5);
                    var force = new Vector3(randomX, 5, randomZ);
                    _rbs[i].AddForce( force * Random.Range(300, 350)* Time.deltaTime, ForceMode.Impulse);
                }
            }
        }
    }
}
