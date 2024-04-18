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

        _movementStateMachine.isFastToRun = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return;
        }
        // 摁移动键
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 || _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            // 摁前进键加空格并且前方有墙壁并且角度满足条件
            if (_movementStateMachine.MoveInputInfo.MoveForwardInput && 
                _movementStateMachine.MoveInputInfo.JumpInput &&
                _movementStateMachine.hasWallOnForward &&
                _movementStateMachine.cameraForwardWithWallAbnormalAngle < _movementStateMachine.climbMaxAngle)
            {
                stateMachine.ChangeState(_movementStateMachine.ClimbState);
                return;
            }
            stateMachine.ChangeState(_movementStateMachine.WalkState);
            return;
        }
        // 摁跳跃键
        if (_movementStateMachine.MoveInputInfo.JumpInput)
        {
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return;
        }
        // 摁下蹲键
        if (_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.SquatState);
        }
    }
}
