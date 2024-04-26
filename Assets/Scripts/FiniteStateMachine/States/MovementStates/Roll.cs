using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    // roll状态下根据状态机中roll最大移动速度计算出的刚体在XOZ平面的移动速度
    private Vector3 _rollVelocity;
    // 进行_rollVelocity速度计算时用到的比率参数
    private float _calRollVelocityRate;
    
    public Roll(StateMachine stateMachine) : base(E_State.Roll, stateMachine)
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
        // 设置最大移动速度
        _movementStateMachine.nowMoveSpeed = _movementStateMachine.rollSpeed;
        // 设置移动方向
        SetDirection();
        // 设置移动速度
        // SetRollVelocity();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        // 驱动玩家移动
        _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.rollForce * _movementStateMachine.direction);
        _movementStateMachine.ClampXozVelocity();
        
        // _movementStateMachine.playerRigidbody.velocity = _rollVelocity;
    }

    private void SetDirection()
    {
        if ((_movementStateMachine.MoveInputInfo.VerticalInput == 0 && _movementStateMachine.MoveInputInfo.HorizontalInput == 0) || preState.state == E_State.Sliding)
        {
            _movementStateMachine.direction = _movementStateMachine.playerTransform.forward;
        }
    }

    private void SetRollVelocity()
    {
        _calRollVelocityRate = _movementStateMachine.rollSpeed /
                               Vector2.Distance(Vector2.zero,
                                   new Vector2(_movementStateMachine.direction.x, _movementStateMachine.direction.z));
        _rollVelocity = new Vector3(_movementStateMachine.direction.x * _calRollVelocityRate,
            _movementStateMachine.playerRigidbody.velocity.y, _movementStateMachine.direction.z * _calRollVelocityRate);
    }

    public void ExitRollState()
    {
        switch (preState.state)
        {
            case E_State.Idle:
                if (_movementStateMachine.ChangeState(_movementStateMachine.IdleState))
                {
                    // 清空水平速度
                    _movementStateMachine.playerRigidbody.velocity =
                        new Vector3(0, _movementStateMachine.playerRigidbody.velocity.y, 0);
                }
                break;
            case E_State.Walk:
                _movementStateMachine.ChangeState(_movementStateMachine.WalkState);
                break;
            case E_State.Run:
                _movementStateMachine.ChangeState(_movementStateMachine.RunState);
                break;
            case E_State.Sliding:
                _movementStateMachine.ChangeState(_movementStateMachine.RunState);
                break;
        }
    }
}
