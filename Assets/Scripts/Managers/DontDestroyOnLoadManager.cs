using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    private static DontDestroyOnLoadManager _instance;
    public static DontDestroyOnLoadManager Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
