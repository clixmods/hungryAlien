using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioAliase;

public class CollisionSurface : MonoBehaviour
{
    [SerializeField, Aliase] private string aliaseCollision;

    public void Play()
    {
        AudioManager.PlaySoundAtPosition(aliaseCollision, transform.position);
    }
}
