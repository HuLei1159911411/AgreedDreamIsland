using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fight : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    public Weapon FightWeapon;
    
    public Fight(StateMachine stateMachine) : base(E_State.Fight, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }
}
