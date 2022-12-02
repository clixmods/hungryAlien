using Level;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourcheEnigme : MonoBehaviour
{
    [SerializeField] GameObject[] _books;

    private void OnTriggerEnter(Collider other)
    {
        
        if(other.gameObject.layer == 9)
        {
            other.gameObject.SetActive(false);
            for (int i = 0; i < _books.Length; i++)
            {
                _books[i].gameObject.SetActive(true);
                _books[i].transform.position = other.gameObject.transform.position;
                _books[i].GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-5, 5),Random.Range(-5, 5),Random.Range(-5, 5)).normalized*Random.Range(0,5), ForceMode.Impulse);
            }
            
        }
    }
}
