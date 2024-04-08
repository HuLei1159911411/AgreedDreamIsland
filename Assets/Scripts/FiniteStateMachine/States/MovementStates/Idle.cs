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

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        // 在移动状态机中更新输入信息
        _movementStateMachine.UpdateMoveInputInformation();

        // 更新检查是否在地面
        _movementStateMachine.UpdateIsOnGround();
        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return;
        }
        if (_movementStateMachine.MoveInputInfo.JumpInput)
        {
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return;
        }
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 || _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            stateMachine.ChangeState(_movementStateMachine.WalkState);
            return;
        }
    }
}
