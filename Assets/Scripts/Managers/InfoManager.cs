using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
    [Header("影响摄像机遮挡的图层")] public List<LayerMask> layersCameraCheck;
    public LayerMask layerCameraCheck;
    [Header("用来检测是否在地面的射线检测的图层")] public List<LayerMask> layersGroundCheck;
    public LayerMask layerGroundCheck;
    [Header("用来检测墙壁的射线检测的图层")] public List<LayerMask> layersWallCheck;
    public LayerMask layerWallCheck;
    [Header("用来检测可供钩锁勾住的勾中点的图层")] public List<LayerMask> layersGrapplingHookCheck;
    public LayerMask layerGrapplingHookCheck; 
    
    // 遍历用参数
    private int _count;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        for (_count = 0; _count < layersCameraCheck.Count; _count++)
        {
            layerCameraCheck |= layersCameraCheck[_count];
        }
        
        for (_count = 0; _count < layersGroundCheck.Count; _count++)
        {
            layerGroundCheck |= layersGroundCheck[_count];
        }
        
        for (_count = 0; _count < layersWallCheck.Count; _count++)
        {
            layerWallCheck |= layersWallCheck[_count];
        }
        
        for (_count = 0; _count < layersGrapplingHookCheck.Count; _count++)
        {
            layerGrapplingHookCheck |= layersGrapplingHookCheck[_count];
        }
    }
}