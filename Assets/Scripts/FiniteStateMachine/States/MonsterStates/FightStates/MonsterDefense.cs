using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDefense : MonsterState
{
    public bool isDefensing;
    public bool isSuccessfulDefense;
    public MonsterDefense(StateMachine stateMachine) : base(E_State.Defense, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        isDefensing = false;
        isSuccessfulDefense = false;
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDefense"]);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (isSuccessfulDefense)
        {
            _monsterStateMachine.AttackState.AttackType = E_AttackType.StrongAttack;
            _monsterStateMachine.ChangeState(_monsterStateMachine.AttackState);
        }
    }
}
