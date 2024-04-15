using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 用来保存与左或右墙壁相切的方向向量
    private Vector3 _wallForward;
    // 用来控制玩家滑墙的最大时间的计时器
    private float _timer;
    // 经过计算后更符合玩家面朝向与在墙壁上移动方向情况的驱动玩家在墙上运动的力的大小
    private float _calWallRunningForce;
    
    public WallRunning(StateMachine stateMachine) : base("WallRunning", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _timer = 0;
        CloseGravity();
    }

    public override void Exit()
    {
        base.Exit();

        _movementStateMachine.playerRigidbody.useGravity = true;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (ListenInputToChangeState())
        {
            return;
        }
        
        UpdateDirectionWithSpeedWithForce();
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();

        if (Math.Abs(_movementStateMachine.MoveInputInfo.VerticalInput - 1f) < Single.Epsilon)
        {
            // 通过加力驱动在墙壁上移动
            _movementStateMachine.playerRigidbody.AddForce(_calWallRunningForce *
                                                           _movementStateMachine.nowMoveSpeed *
                                                           _movementStateMachine.direction);
        }
        
        _movementStateMachine.ClampXozVelocity();
    }

    private bool ListenInputToChangeState()
    {
        _timer += Time.deltaTime;
        
        if (!_movementStateMachine.hasWallOnLeft && 
            !_movementStateMachine.hasWallOnRight &&
            !_movementStateMachine.hasWallOnForward)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
            return true;
        }

        if (_timer < _movementStateMachine.wallRunningTime && _movementStateMachine.MoveInputInfo.JumpInput)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.JumpState);
        }
        
        if (_timer >= _movementStateMachine.wallRunningTime || 
            !_movementStateMachine.MoveInputInfo.MoveForwardInput
            )
        {
            _movementStateMachine.playerRigidbody.useGravity = true;
        }
        else
        {
            CloseGravity();
        }
        return false;
    }

    private void UpdateDirectionWithSpeedWithForce()
    {
        // 更新当前墙壁的方向向量
        _wallForward = _movementStateMachine.GetWallForward();
        // 更新移动方向(若当前人物的面朝向与墙壁的前进向量方向更接近则两向量相加的向量的模长的值大于两向量相减的向量的模长的值，
        // 可以想象人物面朝向与墙壁前进方向完全相同的情况，这样两个向量相加等于人物面朝向向量的值乘2，模长同时也乘2，而相减则为0)
        _movementStateMachine.direction = (_movementStateMachine.playerTransform.forward + _wallForward).magnitude >
                                          (_movementStateMachine.playerTransform.forward - _wallForward).magnitude
            ? _wallForward
            : -_wallForward;
        // 更新移动速度
        _movementStateMachine.nowMoveSpeed = _movementStateMachine.wallRunningForwardSpeed;
        // 修改滑墙力的大小
        // 将这个方向与玩家面朝向的方向进行夹角计算，使其滑墙力的大小与前计算夹角成反相关关系，当夹角越小说明越贴近移动方向，所以大小越大
        _calWallRunningForce = _movementStateMachine.wallRunningForce * ((90f - Vector3.Angle(
            _movementStateMachine.playerTransform.forward,
            _movementStateMachine.direction)) / 90f);
    }

    private void CloseGravity()
    {
        // 清空Y轴速度
        _movementStateMachine.playerRigidbody.velocity = new Vector3(_movementStateMachine.playerRigidbody.velocity.x,
            0, _movementStateMachine.playerRigidbody.velocity.z);
    }
}
