using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    private static InfoManager _instance;
    public static InfoManager Instance => _instance;

    [Header("地面图层")] public LayerMask layerGround;
    [Header("可以滑行的墙图层")] public LayerMask layerWall;
    [Header("地面阻力")] public float groundDrag;
    [Header("空气阻力")] public float airDrag;
    [Header("玩家最高高度")] public float maxHigh = 100f;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
    }
}