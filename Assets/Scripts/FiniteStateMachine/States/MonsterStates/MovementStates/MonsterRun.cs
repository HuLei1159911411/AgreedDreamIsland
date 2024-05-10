using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRun : MonsterState
{
    public MonsterRun(StateMachine stateMachine) : base(E_State.Run, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();

        if (_monsterStateMachine.CheckNowAngle())
        {
            _monsterStateMachine.ChangeState(_monsterStateMachine.TurnState);
        }
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToRun"]);
    }
    
}
