using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWalk : MonsterState
{
    public MonsterWalk(StateMachine stateMachine) : base(E_State.Walk, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        if (_monsterStateMachine.CheckNowAngle())
        {
            _monsterStateMachine.ChangeState(_monsterStateMachine.TurnState);
        }
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToWalk"]);
    }
}
