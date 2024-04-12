using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    private static InfoManager _instance;
    public static InfoManager Instance => _instance;
    
    // 地面所在层
    public LayerMask layerGround;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
    }
}
