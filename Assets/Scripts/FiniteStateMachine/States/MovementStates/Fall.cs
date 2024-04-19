using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fall : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    // 是否是快速降落
    private bool _isFastFall;

    // 设置的Fall状态下水平方向的最大移动速度
    private float _fallSpeed;

    // 是否松开过下蹲或滑铲键
    private bool _isReleaseSquatInput;

    public Fall(StateMachine stateMachine) : base(E_State.Fall, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _isFastFall = false;
        _isReleaseSquatInput = false;
        _fallSpeed = _movementStateMachine.fallSpeed;

        SetFallSpeedByState(preState);

        _movementStateMachine.nowMoveSpeed = _movementStateMachine.fallSpeed;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (ListenInputToChangeState())
        {
            return;
        }

        UpdateDirection();
    }

    public override void UpdatePhysic()
    {
        if (_isFastFall)
        {
            _movementStateMachine.playerRigidbody.AddForce((_movementStateMachine.fallGravityScale - 1f) *
                                                           _movementStateMachine.playerRigidbody.mass * 9.18f *
                                                           Vector3.down);
        }

        _movementStateMachine.playerRigidbody.AddForce((InfoManager.Instance.groundDrag + 1) *
                                                       _movementStateMachine.direction);
        _movementStateMachine.ClampXozVelocity();
    }

    public override void Exit()
    {
        base.Exit();
        _movementStateMachine.fallSpeed = _fallSpeed;
    }

    private bool ListenInputToChangeState()
    {
        // 判断是不是松开了蹲下或滑铲键(当蹲着或滑铲进入下落状态则可能存在该种情况)
        if (!_movementStateMachine.MoveInputInfo.SquatInput)
        {
            _isReleaseSquatInput = true;
        }

        // 快速下落
        if (_isReleaseSquatInput && _movementStateMachine.MoveInputInfo.SquatInput)
        {
            _isFastFall = true;
        }

        // 前方有墙壁
        if (_movementStateMachine.hasWallOnForward)
        {
            // 前一状态不是WallRunning并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度切换为滑墙状态
            if (preState.state != E_State.WallRunning && _movementStateMachine.cameraForwardWithWallAbnormalAngle >=
                _movementStateMachine.climbMaxAngle &&
                _movementStateMachine.nowHigh >= _movementStateMachine.wallRunningMinHigh)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
                return true;
            }

            // 前一状态不是Climb并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度切换为滑墙状态
            if (preState.state != E_State.Climb && _movementStateMachine.MoveInputInfo.JumpInput &&
                _movementStateMachine.cameraForwardWithWallAbnormalAngle < _movementStateMachine.climbMaxAngle)
            {
                // 摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度小于最大角度切换为攀爬状态
                _movementStateMachine.ChangeState(_movementStateMachine.ClimbState);
                return true;
            }
        }
        
        // 左右两边有墙壁
        if (preState.state != E_State.WallRunning &&
            (_movementStateMachine.hasWallOnLeft && _movementStateMachine.MoveInputInfo.MoveLeftInput ||
             _movementStateMachine.hasWallOnRight && _movementStateMachine.MoveInputInfo.MoveRightInput))
        {
            _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
            return true;
        }

        // 落地
        if (_movementStateMachine.isOnGround)
        {
            if (_isFastFall || !_movementStateMachine.isFastToRun)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
            }
            else
            {
                _movementStateMachine.ChangeState(_movementStateMachine.RunState);
            } 
            return true;
        }

        return false;
    }

    private void UpdateDirection()
    {
        // 更新移动方向
        _movementStateMachine.direction = _movementStateMachine.playerTransform.forward *
                                          _movementStateMachine.MoveInputInfo.VerticalInput;
        _movementStateMachine.direction += _movementStateMachine.playerTransform.right *
                                           _movementStateMachine.MoveInputInfo.HorizontalInput;
        _movementStateMachine.direction = _movementStateMachine.direction.normalized;
    }

    private void SetFallSpeedByState(BaseState State)
    {
        switch (State.state)
        {
            case E_State.Walk:
                _movementStateMachine.fallSpeed = _movementStateMachine.walkHorizontalSpeed;
                break;
            case E_State.Run:
                _movementStateMachine.fallSpeed = _movementStateMachine.walkHorizontalSpeed;
                break;
            case E_State.Squat:
                _movementStateMachine.fallSpeed = _movementStateMachine.squatSpeed;
                break;
            case E_State.Sliding:
                _movementStateMachine.fallSpeed = _movementStateMachine.slidingSpeed;
                break;
            case E_State.Jump:
                SetFallSpeedByState(State.preState);
                break;
            case E_State.WallRunning:
                _movementStateMachine.fallSpeed = _movementStateMachine.wallRunningForwardSpeed;
                break;
            case E_State.Climb:
                _movementStateMachine.fallSpeed = _movementStateMachine.climbHorizontalSpeed;
                break;
        }
    }
}