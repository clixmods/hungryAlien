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
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == objectToDestroy.gameObject)
        {
            if(destroyObjectToDestroy)
                other.gameObject.SetActive(false);

            _isTriggered = true;
           
            for (int i = 0; i < objectToEnables.Length; i++)
            {
                objectToEnables[i].gameObject.SetActive(true);
                objectToEnables[i].GetComponent<IAbsorbable>().InitialPosition = objectToEnables[i].transform.position;
                //objectToEnables[i].GetComponent<IAbsorbable>().WakeObject();
                if (pushObjectOnTriggered)
                {
                    objectToEnables[i].transform.position = other.gameObject.transform.position;
                    var randomX = Random.Range(-5, 5);
                    var randomY = Random.Range(-5, 5);
                    var randomZ = Random.Range(-5, 5);
                    var force = new Vector3(randomX, randomY, randomZ);
                    _rbs[i].AddForce(force.normalized * Random.Range(0, 5), ForceMode.Impulse);
                }
                
                
            }
            
        }
    }
}
