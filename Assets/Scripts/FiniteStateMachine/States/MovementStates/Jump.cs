using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Jump : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    private Vector3 _velocity;
    
    // 是否监听脱离地面
    private bool _isListenLeftGround;
    // 是否脱离过地面
    private bool _hasLeftGround;
    
    public Jump(StateMachine stateMachine) : base(E_State.Jump, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _isListenLeftGround = true;
        _hasLeftGround = _movementStateMachine.isOnGround;
        
        DoJump();
    }

    public override void Exit()
    {
        base.Exit();
        
    }

    public override void UpdateLogic()
    {
        base.UpdatePhysic();

        if (_isListenLeftGround)
        {
            if (!_movementStateMachine.isOnGround)
            {
                _hasLeftGround = true;
                _isListenLeftGround = false;
            }
        }
        
        // 前方有墙壁
        if (_movementStateMachine.hasWallOnForward && _isListenLeftGround)
        {
            // // 前一状态不是WallRunning并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度并且高度满足最低高度要求，切换为滑墙状态
            // if (preState.state != E_State.WallRunning &&
            //     _movementStateMachine.cameraForwardWithWallAbnormalAngle > _movementStateMachine.climbMaxAngle
            //     && _movementStateMachine.nowHigh >= _movementStateMachine.wallRunningMinHigh)
            // {
            //     _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
            //     return;
            // }

            // 前一状态不是Climb，并且摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度，并且摁下前进键，切换为攀爬状态
            if (preState.state != E_State.Climb &&
                _movementStateMachine.cameraForwardWithWallAbnormalAngle <= _movementStateMachine.climbMaxAngle &&
                _movementStateMachine.MoveInputInfo.VerticalInput == 1)
            {
                // 摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度小于最大角度切换为攀爬状态
                _movementStateMachine.ChangeState(_movementStateMachine.ClimbState);
                return;
            }
        }
        
        // 左右两边有墙壁
        if (_movementStateMachine.hasWallOnLeft && _movementStateMachine.MoveInputInfo.MoveLeftInput ||
              _movementStateMachine.hasWallOnRight && _movementStateMachine.MoveInputInfo.MoveRightInput)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
            return;
        }
        
        // 当向上速度小于等于0时自动转换为Fall状态
        if (_movementStateMachine.isOnSlope && _movementStateMachine.playerXozSpeed < 0.1f && _hasLeftGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return;
        }
        if (_movementStateMachine.playerRigidbody.velocity.y <= float.Epsilon && _hasLeftGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return;
        }
    }
    
    private void DoJump()
    {
        // 确保重力开启
        _movementStateMachine.playerRigidbody.useGravity = true;
        // 利用公式h = 1 /2 * g * t^2 和 F * t = m * v得
        // 在墙上跳跃时特殊处理
        if (preState.state == E_State.WallRunning || preState.state == E_State.Climb)
        {
            // 清空速度
            _movementStateMachine.playerRigidbody.velocity = Vector3.zero;
            _movementStateMachine.playerRigidbody.AddForce(
                Mathf.Sqrt(_movementStateMachine.jumpHigh * (Physics.gravity.y) * (-2)) *
                _movementStateMachine.playerRigidbody.mass * (Vector3.up + _movementStateMachine.GetWallNormal()).normalized,
                ForceMode.Impulse);
        }
        else
        {
            _movementStateMachine.playerRigidbody.AddForce(
                Mathf.Sqrt(_movementStateMachine.jumpHigh * (Physics.gravity.y) * (-2)) *
                _movementStateMachine.playerRigidbody.mass * Vector3.up, ForceMode.Impulse);
        }
        
    }
}
