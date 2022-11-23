using System;
using UnityEngine;

namespace Level
{
    public class CustomCollider : MeshCollider
    {
        private IAbsorbable component;

        private void OnCollisionEnter(Collision collision)
        {
            throw new NotImplementedException();
        }

        private void OnCollisionExit(Collision collisionInfo)
        {
            throw new NotImplementedException();
        }

        private void OnCollisionStay(Collision collisionInfo)
        {
            throw new NotImplementedException();
        }
    }
    
}