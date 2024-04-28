using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public class GrapplingHook : MonoBehaviour
{
    // 发射点
    public Transform hookShootPoint;
    // 勾爪模型
    public Transform hook;
    // 勾爪父节点
    public Transform hookFather;
    
    // 速度
    [Range(0,1f)]
    public float hookMoveSpeed = 0.01f;
    
    // 是否绘制勾爪及伸缩
    public bool isDrawHookAndRope;
    // 是否完成勾爪运动
    public bool isCompletedMove;
    // 是否在回收勾爪
    public bool isRetractingRope;
    
    private LineRenderer _rope;
    // 目标位置
    private Vector3 _targetPoint;
    // 勾爪运动协程
    private Coroutine _moveCoroutine;
    private void Awake()
    {
        _rope = GetComponent<LineRenderer>();
        
        isCompletedMove = true;
        isDrawHookAndRope = false;
        isRetractingRope = false;
        
        HideGrapplingHook();
    }

    private void LateUpdate()
    {
        if (isDrawHookAndRope)
        {
            DrawRope();
        }
    }

    private void DrawRope()
    {
        // 更新绳子尾部位置
        _rope.SetPosition(0, hookShootPoint.position);
        // 更新绳子头部位置
        _rope.SetPosition(1, hookFather.position);
    }
    
    public bool ShootHook(Vector3 targetPoint)
    {
        if (!isCompletedMove)
        {
            return false;
        }
        ShowGrapplingHook();
        _targetPoint = targetPoint;
        // 旋转箭头
        hookFather.rotation = Quaternion.LookRotation(targetPoint - hookFather.position);
        _moveCoroutine = StartCoroutine(ToTargetPoint());
        return true;
    }

    public void RetractRope()
    {
        if (isRetractingRope)
        {
            return;
        }

        if (!isCompletedMove)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
        
        isRetractingRope = true;
        _targetPoint = hookShootPoint.position;
        _moveCoroutine = StartCoroutine(ToTargetPoint());
    }
    
    private IEnumerator ToTargetPoint()
    {
        isCompletedMove = false;
        
        while (Vector3.Distance(hookFather.position, _targetPoint) > 0.2f)
        {
            if (isRetractingRope)
            {
                _targetPoint = hookShootPoint.position;
            }
            hookFather.position = Vector3.Lerp(hookFather.position, _targetPoint, hookMoveSpeed);
            yield return null;
        }
        
        hookFather.position = _targetPoint;
        isCompletedMove = true;
        
        if (isRetractingRope)
        {
            HideGrapplingHook();
            isRetractingRope = false;
        }
    }

    // 是否是勾中状态或飞行到勾中点状态
    public bool IsGrapplingHookLocked()
    {
        return isDrawHookAndRope && !isRetractingRope;
    }

    private void HideGrapplingHook()
    {
        hookFather.position = hookShootPoint.position;
        hook.gameObject.SetActive(false);
        
        _rope.SetPosition(0, hookShootPoint.position);
        _rope.SetPosition(1, hookShootPoint.position);
        _rope.enabled = false;
        
        isDrawHookAndRope = false;
    }

    private void ShowGrapplingHook()
    {
        isDrawHookAndRope = true;

        hookFather.position = hookShootPoint.position;
        hook.gameObject.SetActive(true);

        _rope.SetPosition(0, hookShootPoint.position);
        _rope.SetPosition(1, hookShootPoint.position);
        _rope.enabled = true;
    }
    
} 