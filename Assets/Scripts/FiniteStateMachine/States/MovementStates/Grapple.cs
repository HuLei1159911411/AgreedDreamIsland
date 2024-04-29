using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 计算从钩锁起始点到目标点所需提供速度的大小时所用到的参数
    private Vector3 _velocityY;
    private Vector3 _velocityXZ;
    private float _differenceHighStartAndEnd;
    private float _gravity;
    private float _distanceStartAndEndX;
    private float _distanceStartAndEndZ;
    private float _moveTime;
    // 在为速度赋值时临时用来保存计算结果的变量
    private Vector3 _calculateVelocity;

    public bool IsMoveToLeftHookCheckPoint;
    public bool IsMoveToRightHookCheckPoint;
    
    public Grapple(StateMachine stateMachine) : base(E_State.Grapple, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _gravity = Mathf.Abs(Physics.gravity.y);
        SetVelocity();
    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (_movementStateMachine.leftGrapplingHook.isRetractingRope)
        {
            IsMoveToLeftHookCheckPoint = false;
        }

        if (_movementStateMachine.rightGrapplingHook.isRetractingRope)
        {
            IsMoveToRightHookCheckPoint = false;
        }

        if (_movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.playerTransform.forward *
                                                           (_movementStateMachine.MoveInputInfo.VerticalInput *
                                                            Time.deltaTime));
        }

        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0)
        {
            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.playerTransform.right *
                                                           (_movementStateMachine.MoveInputInfo.HorizontalInput *
                                                           Time.deltaTime));
        }
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        
    }

    // 计算从钩锁起始点到目标点所需提供速度的大小
    private Vector3 CalculateJumpVelocity(Vector3 start, Vector3 end, float differenceHighSecondAndHighest)
    {
        _differenceHighStartAndEnd = end.y - start.y - _movementStateMachine.playerHeight * 0.5f;
        if (start.y < end.y)
        {
            differenceHighSecondAndHighest += Mathf.Abs(_differenceHighStartAndEnd);
        }
        
        _distanceStartAndEndX = end.x - start.x;
        _distanceStartAndEndZ = end.z - start.z;
        
        _velocityY = new Vector3(0, Mathf.Sqrt(2 * _gravity * differenceHighSecondAndHighest), 0);
        
        _moveTime = (_velocityY.y / _gravity) +
                    (Mathf.Sqrt(2 * (differenceHighSecondAndHighest - _differenceHighStartAndEnd) / _gravity));
        
        _velocityXZ = new Vector3(_distanceStartAndEndX / _moveTime, 0, _distanceStartAndEndZ / _moveTime);
        
        return _velocityY + _velocityXZ * _movementStateMachine.grapplingHookRopeMoveVelocityRatio;
    }

    public void SetVelocity()
    {
        _movementStateMachine.playerRigidbody.velocity = Vector3.zero;

        // 两个钩锁
        if (_movementStateMachine.leftGrapplingHook.IsGrapplingHookLocked() &&
            !IsMoveToLeftHookCheckPoint &&
            _movementStateMachine.rightGrapplingHook.IsGrapplingHookLocked() &&
            !IsMoveToRightHookCheckPoint)
        {
            _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                _movementStateMachine.playerTransform.position,
                (_movementStateMachine.leftGrapplingHook.TargetPoint +
                 _movementStateMachine.rightGrapplingHook.TargetPoint) * 0.5f,
                _movementStateMachine.grapplingHighestPointRelativeHigh);
            
            /*
            // 左边的比较高
            if (_movementStateMachine.leftGrapplingHook.TargetPoint.y >
                _movementStateMachine.rightGrapplingHook.TargetPoint.y)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position,
                    _movementStateMachine.leftGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                
                _calculateVelocity = CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position, 
                    _movementStateMachine.rightGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);

                _movementStateMachine.playerRigidbody.velocity += 
                    new Vector3(_calculateVelocity.x, 0, _calculateVelocity.z);
            }
            // 右边的比较高
            else
            {
                _calculateVelocity = _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position,
                    _movementStateMachine.leftGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);

                _movementStateMachine.playerRigidbody.velocity +=
                    new Vector3(_calculateVelocity.x, 0, _calculateVelocity.z);
                
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position, 
                    _movementStateMachine.rightGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
            }
            */
            
            IsMoveToLeftHookCheckPoint = true;
            IsMoveToRightHookCheckPoint = true;
        }
        // 单个钩锁
        else
        {
            if (_movementStateMachine.leftGrapplingHook.IsGrapplingHookLocked() &&
                !IsMoveToLeftHookCheckPoint)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position,
                    _movementStateMachine.leftGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                IsMoveToLeftHookCheckPoint = true;
            }

            if (_movementStateMachine.rightGrapplingHook.IsGrapplingHookLocked() &&
                !IsMoveToRightHookCheckPoint)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position, 
                    _movementStateMachine.rightGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                IsMoveToRightHookCheckPoint = true;
            }
        }
    }
}
