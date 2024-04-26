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
    
    public WallRunning(StateMachine stateMachine) : base(E_State.WallRunning, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
            preState = _movementStateMachine.GetInitialState();
        }
    }

    public override void Enter()
    {
        base.Enter();
        // 前一状态时Climb状态则继承其攀爬的时间
        if (preState.state == E_State.Climb)
        {
            _timer = (preState as Climb).ClimbTimer;
        }
        else
        {
            _timer = 0;
        }
        
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

        if (_timer < _movementStateMachine.wallRunningTime && _movementStateMachine.MoveInputInfo.MoveForwardInput)
        {
            // 加力将使玩家贴近墙壁
            _movementStateMachine.playerRigidbody.AddForce(-_movementStateMachine.GetWallNormal() * 10f);
        }
    }

    private bool ListenInputToChangeState()
    {
        _timer += Time.deltaTime;
        
        // 边上没有墙或者当前高度已经下降到地面了
        if (_movementStateMachine.nowHigh < _movementStateMachine.wallRunningMinHigh || 
            (!_movementStateMachine.hasWallOnLeft && 
            !_movementStateMachine.hasWallOnRight &&
            !_movementStateMachine.hasWallOnForward))
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }
        
        if (_movementStateMachine.MoveInputInfo.JumpInput)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.JumpState);
            return true;
        }
        
        if (_timer >= _movementStateMachine.wallRunningTime || 
            !_movementStateMachine.MoveInputInfo.MoveForwardInput)
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
        // 更新移动方向(若当前人物的面朝向(摄像机面朝向)与墙壁的前进向量方向更接近则两向量相加的向量的模长的值大于两向量相减的向量的模长的值，
        // 可以想象人物面朝向(摄像机面朝向)与墙壁前进方向完全相同的情况，这样两个向量相加等于人物面朝向(摄像机面朝向)向量的值乘2，模长同时也乘2，而相减则为0)
        _movementStateMachine.direction = (CameraController.Instance.transform.forward + _wallForward).magnitude >
                                          (CameraController.Instance.transform.forward - _wallForward).magnitude
            ? _wallForward
            : -_wallForward;
        // 更新移动速度
        _movementStateMachine.nowMoveSpeed = _movementStateMachine.wallRunningForwardSpeed;
        // 修改滑墙力的大小
        // 将这个方向与玩家面朝向的方向(摄像机面朝向)进行夹角计算，使其滑墙力的大小与前计算夹角成反相关关系，当夹角越小说明越贴近移动方向，所以大小越大
        _calWallRunningForce = _movementStateMachine.wallRunningForce * ((90f - Vector3.Angle(
            CameraController.Instance.transform.forward,
            _movementStateMachine.direction)) / 90f);
    }

    private void CloseGravity()
    {
        _movementStateMachine.playerRigidbody.useGravity = false; 
        // 清空Y轴速度
        _movementStateMachine.playerRigidbody.velocity = new Vector3(_movementStateMachine.playerRigidbody.velocity.x,
            0, _movementStateMachine.playerRigidbody.velocity.z);
    }
}
