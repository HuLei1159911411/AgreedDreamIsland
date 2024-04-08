using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fall : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
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
        
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _movementStateMachine.UpdateIsOnGround();
        if (_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
    }
}
