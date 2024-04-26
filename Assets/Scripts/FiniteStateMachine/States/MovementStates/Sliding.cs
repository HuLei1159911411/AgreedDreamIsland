using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    // 滑铲加速时间计时器
    private float _timer;

    public Sliding(StateMachine stateMachine) : base(E_State.Sliding, stateMachine)
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

        _timer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (ListenInputToChangeState())
        {
            return;
        }

        UpdateDirectionWithSpeed();
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();

        // 根据移动方向移动
        // Translate移动
        // _movementStateMachine.playerTransform.Translate(_movementStateMachine.nowMoveSpeed * Time.deltaTime * _movementStateMachine.direction,Space.World);
        if ((_movementStateMachine.isOnSlope && !_movementStateMachine.CheckIsSlidingUp()))
        {
            // 通过给力移动
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.slidingMoveForce *
                _movementStateMachine.GetDirectionOnSlope(), ForceMode.Force);
        }
        else
        {
            if (_timer < _movementStateMachine.slidingAccelerateTime ||
                (_movementStateMachine.IsTryToChangeState))
            {
                _timer += Time.fixedDeltaTime;
                // 通过给力移动
                _movementStateMachine.playerRigidbody.AddForce(
                    _movementStateMachine.nowMoveSpeed * _movementStateMachine.slidingMoveForce *
                    _movementStateMachine.direction, ForceMode.Force);
            }
        }

        // 限制玩家最大速度
        _movementStateMachine.ClampXozVelocity();
    }

    private bool ListenInputToChangeState()
    {
        // 在地面上摁跳跃键
        if (_movementStateMachine.isOnGround && _movementStateMachine.MoveInputInfo.JumpInput)
        {
            return stateMachine.ChangeState(_movementStateMachine.JumpState);
        }

        // 滑铲加速未结束(及加速时间未到)
        if (_timer < _movementStateMachine.slidingAccelerateTime && !_movementStateMachine.isOnSlope)
        {
            return false;
        }
        
        // 加速结束后摁奔跑键进入翻滚状态
        if (_movementStateMachine.MoveInputInfo.RunInput)
        {
            return _movementStateMachine.ChangeState(_movementStateMachine.RollState);
        }

        // 结束加速后摁后退键
        if (_movementStateMachine.MoveInputInfo.VerticalInput == -1)
        {
            return _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
        }

        // 未松开滑铲键并且水平速度未接近0则不会改变滑铲状态(若松开滑铲键了速度变接近0则变为Idle状态，若速度不接近0则不改变状态)
        if (_movementStateMachine.playerXozSpeed > 0.3f && _movementStateMachine.MoveInputInfo.SlidingInput)
        {
            return false;
        }
        
        // 滑到空中并且加速结束
        if (!_movementStateMachine.isOnGround && _timer >= _movementStateMachine.slidingAccelerateTime)
        {
            return stateMachine.ChangeState(_movementStateMachine.IdleState);
        }

        // 松开WASD或摁住WS或摁住AD或摁住WASD且水平速度已经快接近0
        if (_movementStateMachine.MoveInputInfo.HorizontalInput == 0 &&
            _movementStateMachine.MoveInputInfo.VerticalInput == 0 &&
            _movementStateMachine.playerXozSpeed <= 0.3f)
        {
            return stateMachine.ChangeState(_movementStateMachine.IdleState);
        }
        else if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 ||
                 _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            return stateMachine.ChangeState(_movementStateMachine.RunState);
        }

        return false;
    }

    private void UpdateDirectionWithSpeed()
    {
        // 更新移动方向
        if (_movementStateMachine.MoveInputInfo.SlidingInput || _movementStateMachine.IsTryToChangeState)
        {
            _movementStateMachine.direction = _movementStateMachine.playerTransform.forward;
        }
        else
        {
            _movementStateMachine.direction = _movementStateMachine.playerTransform.forward *
                                              _movementStateMachine.MoveInputInfo.VerticalInput;
        }

        _movementStateMachine.direction += _movementStateMachine.playerTransform.right *
                                           _movementStateMachine.MoveInputInfo.HorizontalInput;
        _movementStateMachine.direction = _movementStateMachine.direction.normalized;

        // 更新速度
        if (_movementStateMachine.isOnSlope)
        {
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.slidingOnSlopeSpeed *
                                                 (1f + 0.5f * (_movementStateMachine.slopeAngle / 90f));
        }
        else
        {
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.slidingSpeed;
        }
    }
}