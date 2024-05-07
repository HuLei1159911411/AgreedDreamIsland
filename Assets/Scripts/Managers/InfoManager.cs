using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    private static InfoManager _instance;
    public static InfoManager Instance => _instance;

    [Header("图层信息")]
    [Tooltip("地面图层")] public LayerMask layerGround;
    [Tooltip("墙壁图层")] public LayerMask layerWall;
    [Tooltip("影响摄像机遮挡的图层")] public List<LayerMask> layersCameraCheck;
    [HideInInspector] public LayerMask layerCameraCheck;
    [Tooltip("用来检测是否在地面的射线检测的图层")] public List<LayerMask> layersGroundCheck;
    [HideInInspector] public LayerMask layerGroundCheck;
    [Tooltip("用来检测墙壁的射线检测的图层")] public List<LayerMask> layersWallCheck;
    [HideInInspector] public LayerMask layerWallCheck;
    [Tooltip("用来检测可供钩锁勾住的勾中点的图层")] public List<LayerMask> layersGrapplingHookCheck;
    [HideInInspector] public LayerMask layerGrapplingHookCheck; 
    
    [Header("数据信息")]
    [Tooltip("地面阻力")] public float groundDrag;
    [Tooltip("空气阻力")] public float airDrag;
    [Tooltip("玩家最高高度")] public float maxHigh = 100f;
    
    [Header("设置信息")]
    [Tooltip("是否在攻击时锁定方向")] public bool isLockAttackDirection;
    [Tooltip("是否在攻击时自动锁定敌人")] public bool isAutoLockEnemy;
    
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