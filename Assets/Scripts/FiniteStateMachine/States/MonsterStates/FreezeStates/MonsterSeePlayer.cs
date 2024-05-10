using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSeePlayer : MonsterState
{
    public MonsterSeePlayer(StateMachine stateMachine) : base(E_State.SeePlayer, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToSeePlayer"]);
    }
}
