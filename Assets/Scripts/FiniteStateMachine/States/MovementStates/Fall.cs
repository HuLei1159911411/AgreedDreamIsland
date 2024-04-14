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

        if (_movementStateMachine.isOnGround)
        {
            if (_movementStateMachine.PreState.name == "Run")
            {
                _movementStateMachine.ChangeState(_movementStateMachine.WalkState);
            }
            else
            {
                _movementStateMachine.ChangeState(_movementStateMachine.PreState);
            }
            
        }
    }

    public override void Exit()
    {
        base.Exit();
        
    }
}
