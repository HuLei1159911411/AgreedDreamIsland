using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDeath : MonsterState
{
    public MonsterDeath(StateMachine stateMachine) : base(E_State.Death, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        _monsterStateMachine.monsterRigidbody.useGravity = false;
        _monsterStateMachine.monsterCollider.enabled = false;
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDeath"]);
    }
}
