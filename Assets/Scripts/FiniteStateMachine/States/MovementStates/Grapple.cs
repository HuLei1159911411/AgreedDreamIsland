using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    
    public Grapple(StateMachine stateMachine) : base(E_State.Grapple, stateMachine)
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
        
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        
    }
}
