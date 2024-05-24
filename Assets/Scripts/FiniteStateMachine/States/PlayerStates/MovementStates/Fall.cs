using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fall : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    // 是否是快速降落
    public bool isFastFall;

    // 是否松开过下蹲或滑铲键
    private bool _isReleaseSquatInput;

    public Fall(StateMachine stateMachine) : base(E_State.Fall, stateMachine)
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
        isFastFall = false;
        _isReleaseSquatInput = false;

        SetNowSpeedByState();
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
        if (isFastFall)
        {
            _movementStateMachine.playerRigidbody.AddForce((_movementStateMachine.fallGravityScale - 1f) *
                                                           _movementStateMachine.playerRigidbody.mass * 9.18f *
                                                           Vector3.down);
        }

        _movementStateMachine.playerRigidbody.AddForce((InfoManager.Instance.airDrag + 10) *
                                                       _movementStateMachine.direction);
        _movementStateMachine.ClampXozVelocity();
    }

    private bool ListenInputToChangeState()
    {
        // 判断是不是松开了蹲下或滑铲键(当蹲着或滑铲进入下落状态则可能存在该种情况)
        if (!_movementStateMachine.MoveInputInfo.SquatInput)
        {
            _isReleaseSquatInput = true;
        }

        // 快速下落
        if (_isReleaseSquatInput && _movementStateMachine.MoveInputInfo.SquatInput && preState.state != E_State.Grapple)
        {
            isFastFall = true;
        }
        
        // 落地
        if (_movementStateMachine.isOnGround)
        {
            if (isFastFall)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
            }
            else if(_movementStateMachine.isFastToRun && _movementStateMachine.MoveInputInfo.VerticalInput == 1)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.RunState);
            } else if (_movementStateMachine.MoveInputInfo.VerticalInput == 1)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.WalkState);
            }
            else
            {
                _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
            }
            return true;
        }

        // 前方有墙壁
        if (_movementStateMachine.hasWallOnForward)
        {
            // 前一状态不是WallRunning并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度切换为滑墙状态
            // if (preState.state != E_State.WallRunning && _movementStateMachine.cameraForwardWithWallAbnormalAngle >
            //     _movementStateMachine.climbMaxAngle &&
            //     _movementStateMachine.nowHigh >= _movementStateMachine.wallRunningMinHigh)
            // {
            //     _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
            //     return true;
            // }

            // 前一状态不是Climb，并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度，并且摁下前进键，切换为攀爬状态
            if (preState.state != E_State.Climb &&
                _movementStateMachine.cameraForwardWithWallAbnormalAngle <= _movementStateMachine.climbMaxAngle &&
                _movementStateMachine.MoveInputInfo.VerticalInput == 1)
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

    private void SetNowSpeedByState()
    {
        if (preState.state == E_State.Idle || preState.state == E_State.Jump && preState.preState.state == E_State.Idle)
        {
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.fallSpeed;
        }
    }
}