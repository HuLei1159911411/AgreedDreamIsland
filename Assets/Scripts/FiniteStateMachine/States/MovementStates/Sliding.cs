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
        }
    }

    public override void Enter()
    {
        base.Enter();

        _timer = 0f;
        // 将玩家模型压缩一半达到滑铲效果，之后导入模型和动作后改成播放对应动作，这里要改掉
        _movementStateMachine.transform.localScale = new Vector3(_movementStateMachine.transform.localScale.x,
             _movementStateMachine.slidingYScale,
            _movementStateMachine.transform.localScale.z);
        
        
        // 通过给力移动
        _movementStateMachine.playerRigidbody.AddForce(
            _movementStateMachine.nowMoveSpeed * _movementStateMachine.slidingMoveForce *
            _movementStateMachine.direction, ForceMode.Force);
    }

    public override void Exit()
    {
        base.Exit();

        // 让玩家远离地面
        // _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.slidingMoveForce * 30f * Vector3.up);
        _movementStateMachine.playerTransform.position += new Vector3(0,
            _movementStateMachine.playerHeight * (1f - _movementStateMachine.slidingYScale) * 0.5f, 0);
        
        // 将玩家模型还原，之后导入模型和动作后改成播放对应动作，这里要改掉
        _movementStateMachine.transform.localScale = new Vector3(_movementStateMachine.transform.localScale.x,
            1f,
            _movementStateMachine.transform.localScale.z);
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
        if (_movementStateMachine.isOnSlope && !_movementStateMachine.CheckIsSlidingUp())
        {
            // 通过给力移动
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.slidingMoveForce *
                _movementStateMachine.GetDirectionOnSlope(), ForceMode.Force);
        }
        else
        {
            if (_timer < _movementStateMachine.slidingAccelerateTime)
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
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return true;
        }
        
        // 滑铲加速未结束(及加速时间未到)
        if (_timer < _movementStateMachine.slidingAccelerateTime)
        {
            return false;
        }
        
        // 未松开滑铲键并且水平速度未接近0则不会改变滑铲状态(若松开滑铲键了速度变接近0则变为Idle状态，若速度不接近0则不改变状态)
        if (_movementStateMachine.playerXozSpeed > 0.1f && _movementStateMachine.MoveInputInfo.SlidingInput)
        {
            return false;
        }
        
        // 松开WASD或摁住WS或摁住AD或摁住WASD且水平速度已经快接近0
        if (_movementStateMachine.MoveInputInfo.HorizontalInput == 0 &&
            _movementStateMachine.MoveInputInfo.VerticalInput == 0 && 
            _movementStateMachine.playerXozSpeed <= 0.1f)
        {
            stateMachine.ChangeState(_movementStateMachine.IdleState);
            return true;
        }
        else if(_movementStateMachine.MoveInputInfo.HorizontalInput != 0 ||
                _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            stateMachine.ChangeState(_movementStateMachine.RunState);
            return true;
        }

        return false;
    }

    private void UpdateDirectionWithSpeed()
    {
        // 更新移动方向
        _movementStateMachine.direction = _movementStateMachine.playerTransform.forward *
                                          _movementStateMachine.MoveInputInfo.VerticalInput;
        _movementStateMachine.direction += _movementStateMachine.playerTransform.right *
                                           _movementStateMachine.MoveInputInfo.HorizontalInput;
        _movementStateMachine.direction = _movementStateMachine.direction.normalized;
            
        // 更新速度
        if (_movementStateMachine.isOnSlope)
        {
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.slidingOnSlopeSpeed * (1f + 0.5f * (_movementStateMachine.slopeAngle / 90f));
        }
        else
        {
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.slidingSpeed;
        }
    }
}