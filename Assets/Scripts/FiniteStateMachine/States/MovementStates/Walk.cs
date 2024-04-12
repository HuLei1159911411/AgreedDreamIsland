using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    private Vector3 _direction;
    // 玩家摁奔跑键的计时器
    private float _timerPressKey;
    
    public Walk(StateMachine stateMachine) : base("Walk", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        
        // 清空计时器
        _timerPressKey = 0f;
    }

    public override void Exit()
    {
        base.Enter();

        _movementStateMachine.PreState = this;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 更新移动输入信息
        _movementStateMachine.UpdateMoveInputInformation();

        // 更新检查是否在地面
        _movementStateMachine.UpdateIsOnGround();
        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return;
        }
        // 摁跳跃键
        if (_movementStateMachine.MoveInputInfo.JumpInput)
        {
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return;
        }
        
        // 松开WASD或摁住WS或摁住AD或摁住WASD
        if (_movementStateMachine.MoveInputInfo.HorizontalInput == 0 &&
            _movementStateMachine.MoveInputInfo.VerticalInput == 0)
        {
            stateMachine.ChangeState(_movementStateMachine.IdleState);
            return;
        }
        else
        {
            // 更新移动方向
            _direction = _movementStateMachine.playerTransform.forward *
                          _movementStateMachine.MoveInputInfo.VerticalInput;
            _direction += _movementStateMachine.playerTransform.right *
                          _movementStateMachine.MoveInputInfo.HorizontalInput;
            _direction = _direction.normalized;
            // 更新当前方向移动速度(有水平方向移动的输入则以水平速度为主)
            if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0)
            {
                // 更新当前速度为水平速度
                _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkHorizontalSpeed;
            }
            // 保持移动
            else
            {
                if (_movementStateMachine.MoveInputInfo.MoveForwardInput)
                {
                    // 更新当前速度为前进速度
                    _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkForwardSpeed;
                }
                else
                {
                    // 更新当前速度为后退速度
                    _movementStateMachine.nowMoveSpeed = _movementStateMachine.walkBackwardSpeed;
                }
            }
            
            // 根据移动方向移动
            // Translate移动
            // _movementStateMachine.playerTransform.Translate(_movementStateMachine.nowMoveSpeed * Time.deltaTime * _direction,Space.World);
            
            // 切换Run
            if (_movementStateMachine.MoveInputInfo.RunInput)
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
        }
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        // 通过给力移动
        _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.nowMoveSpeed * _movementStateMachine.walkMoveForce * _direction, ForceMode.Force);
        // 限制玩家最大速度
        _movementStateMachine.ClampXozVelocity();
    }
}
