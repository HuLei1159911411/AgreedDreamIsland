using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public float playTime;
    public AudioSource audioSource;

    public void RecyclingSelf()
    {
        ObjectPoolManager.Instance.RecyclingObject(E_ObjectType.SoundEffect, gameObject);
    }
    
}
