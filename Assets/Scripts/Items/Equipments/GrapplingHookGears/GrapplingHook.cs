using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;

public class GrapplingHook : MonoBehaviour
{
    // 钩锁装置
    public GrapplingHookGears grapplingHookGears;
    // 发射点
    public Transform hookShootPoint;
    // 勾爪模型
    public Transform hook;
    // 勾爪父节点
    public Transform hookFather;
    // 是否为左钩锁
    public bool isLeft;
    // 速度
    [Range(0, 1f)] public float hookMoveSpeed = 0.01f;
    // 是否绘制勾爪及伸缩
    public bool isDrawHookAndRope;
    // 是否完成勾爪运动
    public bool isCompletedMove;
    // 是否在回收勾爪
    public bool isRetractingRope;
    // 勾爪目标位置
    public Vector3 TargetPoint => _targetPoint;
    private LineRenderer _rope;

    // 目标位置
    private Vector3 _targetPoint;
    // 勾爪运动协程
    private Coroutine _moveCoroutine;
    // 协程yield return 变量
    private WaitForFixedUpdate _waitForFixedUpdate;
    // 钩锁勾中时间计时器
    private float _timer;
    // 临时保存计算绳索距离的变量
    private float _distance;
    // 保存当前勾爪所对应的springJoint节点
    public SpringJoint springJoint;

    private void OnEnable()
    {
        _rope = GetComponent<LineRenderer>();

        isCompletedMove = true;
        isDrawHookAndRope = false;
        isRetractingRope = false;
        _waitForFixedUpdate = new WaitForFixedUpdate();

        HideGrapplingHook();
    }
    
    private void Update()
    {
        if (isDrawHookAndRope && _timer < 1f)
        {
            _distance = Vector3.Distance(hookShootPoint.position, hookFather.position);
            if (_distance > grapplingHookGears.grapplingHookRopeDestroyLength)
            {
                _timer += Time.deltaTime;
                if (_timer >= 1f)
                {
                    RetractRope();
                }
            }
        }
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

    // 返回钩锁是否勾中锁定(飞行完成与物体接触)
    public bool IsGrapplingHookLocked()
    {
        return isCompletedMove && isDrawHookAndRope;
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
        // 在发射中，或正在回收中，或者并没有进行绘制钩锁绳索不进行操作
        if (!isCompletedMove || isRetractingRope || !isDrawHookAndRope)
        {
            return;
        }

        grapplingHookGears.InitPlayerSpringJoint(isLeft ? 0 : 1);
        
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
                hookFather.position = Vector3.Lerp(hookFather.position, _targetPoint, hookMoveSpeed * 2f);
            }
            else
            {
                hookFather.position = Vector3.Lerp(hookFather.position, _targetPoint, hookMoveSpeed);
            }

            yield return _waitForFixedUpdate;
        }

        if (isRetractingRope)
        {
            HideGrapplingHook();
            isRetractingRope = false;
        }
        else
        {
            springJoint.connectedAnchor = _targetPoint;
            springJoint.maxDistance = grapplingHookGears.grapplingHookMaxLength;
            springJoint.minDistance = grapplingHookGears.grapplingHookMinLength;
            _timer = 0;
        }

        hookFather.position = _targetPoint;
        isCompletedMove = true;
    }

    public void HideGrapplingHook()
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