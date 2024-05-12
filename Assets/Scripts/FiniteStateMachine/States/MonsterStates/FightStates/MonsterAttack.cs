using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum E_AttackType
{
    NormalAttack = 0,
    ComboAttack = 1,
    StrongAttack = 2,
}

public class MonsterAttack : MonsterState
{
    public E_AttackType AttackType;
    private int _randomSelectAttackAnimationCount;
    public MonsterAttack(StateMachine stateMachine) : base(E_State.Attack, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        SelectAttackAnimation();
    }

    private void SelectAttackAnimation()
    {
        switch (AttackType)
        {
            case E_AttackType.NormalAttack:
                _randomSelectAttackAnimationCount = Random.Range(0, _monsterStateMachine.monsterNormalAttackCount);
                
                _monsterStateMachine.animator.SetInteger(_monsterStateMachine.DicAnimatorIndexes["AttackNum"],
                    _randomSelectAttackAnimationCount);
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToNormalAttack"]);
                break;
            case E_AttackType.ComboAttack:
                _randomSelectAttackAnimationCount = Random.Range(0, _monsterStateMachine.monsterComboAttackCount);
                
                _monsterStateMachine.animator.SetInteger(_monsterStateMachine.DicAnimatorIndexes["AttackNum"],
                    _randomSelectAttackAnimationCount);
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToComboAttack"]);
                break;
            case E_AttackType.StrongAttack:
                _randomSelectAttackAnimationCount = Random.Range(0, _monsterStateMachine.monsterStrongAttackCount);
                
                _monsterStateMachine.animator.SetInteger(_monsterStateMachine.DicAnimatorIndexes["AttackNum"],
                    _randomSelectAttackAnimationCount);
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToStrongAttack"]);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
