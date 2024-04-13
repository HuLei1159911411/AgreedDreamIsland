using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    public Idle(StateMachine stateMachine) : base("Idle", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();

        _movementStateMachine.PreState = this;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 在移动状态机中更新输入信息
        _movementStateMachine.UpdateMoveInputInformation();

        // 更新检查是否在地面
        _movementStateMachine.UpdateIsOnGroundWithIsOnSlope();
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
        // 摁移动键
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 || _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            stateMachine.ChangeState(_movementStateMachine.WalkState);
            return;
        }
        // 摁下蹲键
        if (_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.SquatState);
        }
    }
}
