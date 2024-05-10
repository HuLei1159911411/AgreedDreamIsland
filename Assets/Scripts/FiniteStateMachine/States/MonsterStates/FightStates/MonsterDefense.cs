using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDefense : MonsterState
{
    public MonsterDefense(StateMachine stateMachine) : base(E_State.Defense, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDefense"]);
    }
}
