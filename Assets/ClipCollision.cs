using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ClipCollision : MonoBehaviour
{
    #if UNITY_EDITOR
        private BoxCollider[] _boxs;
        [SerializeField] Color color = Color.red;
        public bool EditSize;
        private void OnValidate()
        {
            _boxs = GetComponentsInChildren<BoxCollider>();
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var box in _boxs)
            {
                //Gizmos.color = color;
             
                //Gizmos.DrawCube(box.bounds.center,box.bounds.size);
            }
            
        }
    #endif
}
