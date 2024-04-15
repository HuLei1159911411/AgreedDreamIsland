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
    public Fall(StateMachine stateMachine) : base("Fall", stateMachine)
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
        UpdateDirectionWithSpeed();
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
        if (!_movementStateMachine.MoveInputInfo.SquatInput)
        {
            _isReleaseSquatInput = true;
        }
        if (_isReleaseSquatInput && _movementStateMachine.MoveInputInfo.SquatInput)
        {
            _isFastFall = true;
        }
        if (_movementStateMachine.isOnGround)
        {
            if (_isFastFall)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
            }
            else if(preState.name == "Jump" || preState.name == "Sliding")
            {
                if (preState.name == "Jump" && preState.preState.name == "Sliding")
                {
                    _movementStateMachine.ChangeState(preState.preState.preState);
                }
                else
                {
                    _movementStateMachine.ChangeState(preState.preState);
                }
            }
            else
            {
                _movementStateMachine.ChangeState(preState);
            }
            
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
    }

    private void SetFallSpeedByState(BaseState state)
    {
        switch (state.name)
        {
            case "Walk":
                _movementStateMachine.fallSpeed = _movementStateMachine.walkHorizontalSpeed;
                break;
            case "Run":
                _movementStateMachine.fallSpeed = _movementStateMachine.walkHorizontalSpeed;
                break;
            case "Squat":
                _movementStateMachine.fallSpeed = _movementStateMachine.squatSpeed;
                break;
            case "Sliding":
                _movementStateMachine.fallSpeed = _movementStateMachine.slidingSpeed;
                break;
            case "Jump":
                SetFallSpeedByState(state.preState);
                break;
        }
    }
}
