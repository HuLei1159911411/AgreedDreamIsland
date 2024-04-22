using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    // 玩家摁奔跑键的计时器
    private float _timerPressKey;
    // 跳跃冷却计时器
    private float _timerJump;

    public Walk(StateMachine stateMachine) : base(E_State.Walk, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _timerJump = 0f;
        _movementStateMachine.isFastToRun = false;
        
        // 清空计时器
        _timerPressKey = 0f;
        _movementStateMachine.isFastToRun = false;
    }

    public override void Exit()
    {
        base.Enter();
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
        if (!_movementStateMachine.isOnSlope)
        {
            // 通过给力移动
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.walkMoveForce *
                _movementStateMachine.direction, ForceMode.Force);
        }
        else
        {
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.walkMoveForce *
                _movementStateMachine.GetDirectionOnSlope(), ForceMode.Force);
        }
        // 限制玩家最大速度
        _movementStateMachine.ClampXozVelocity();
    }

    private bool ListenInputToChangeState()
    {
        _timerJump += Time.deltaTime;
        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }

        // 摁前进键加空格并且前方有墙壁并且角度满足条件
        if (_movementStateMachine.MoveInputInfo.MoveForwardInput && 
            _movementStateMachine.MoveInputInfo.JumpInput &&
            _movementStateMachine.hasWallOnForward &&
            _movementStateMachine.cameraForwardWithWallAbnormalAngle < _movementStateMachine.climbMaxAngle)
        {
            stateMachine.ChangeState(_movementStateMachine.ClimbState);
            return true;
        }
        
        // 摁跳跃键
        if (_movementStateMachine.MoveInputInfo.JumpInput && _timerJump > 0.2f)
        {
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return true;
        }

        // 松开WASD或摁住WS或摁住AD或摁住WASD
        if (_movementStateMachine.MoveInputInfo.HorizontalInput == 0 &&
            _movementStateMachine.MoveInputInfo.VerticalInput == 0)
        {
            stateMachine.ChangeState(_movementStateMachine.IdleState);
            return true;
        }
        
        // 摁下蹲键
        if (_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.SquatState);
            return true;
        }
        
        // 是由向后或向左和向右跑步变为的走路摁前进后恢复跑步
        if (preState.state == E_State.Run && _movementStateMachine.MoveInputInfo.VerticalInput == 1)
        {
            stateMachine.ChangeState(_movementStateMachine.RunState);
            return true;
        }
        
        // 切换Run
        if (_movementStateMachine.MoveInputInfo.RunInput && _movementStateMachine.MoveInputInfo.VerticalInput == 1)
        {
            _timerPressKey += Time.deltaTime;
            if (_timerPressKey >= _movementStateMachine.toRunTime)
            {
                _timerPressKey = 0;
                _movementStateMachine.ChangeState(_movementStateMachine.RunState);
            }
        }
        else
        {
            _timerPressKey = 0f;
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
        // 更新当前方向移动速度(水平和垂直方向均有输入则速度为水平速度和垂直速度的较小值)
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 && _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            // 更新当前速度为当前水平方向和垂直方向速度较小值
            // 向前与水平混合
            if (_movementStateMachine.MoveInputInfo.MoveForwardInput)
            {
                _movementStateMachine.nowMoveSpeed =
                    _movementStateMachine.walkForwardSpeed < _movementStateMachine.walkHorizontalSpeed
                        ? _movementStateMachine.walkForwardSpeed
                        : _movementStateMachine.walkHorizontalSpeed;
            }
            // 向后与水平混合
            else
            {
                _movementStateMachine.nowMoveSpeed =
                    _movementStateMachine.walkBackwardSpeed < _movementStateMachine.walkHorizontalSpeed
                        ? _movementStateMachine.walkBackwardSpeed
                        : _movementStateMachine.walkHorizontalSpeed;
            }
        }
        else
        {
            if (_movementStateMachine.MoveInputInfo.VerticalInput == 1)
            {
                // 更新当前速度为前进速度
                _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkForwardSpeed;
            }
            if (_movementStateMachine.MoveInputInfo.VerticalInput == -1)
            {
                // 更新当前速度为后退速度
                _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkBackwardSpeed;
            }

            if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0)
            {
                // 更新当前速度为水平移动速度
                _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkHorizontalSpeed;
            }
        }
    }
}