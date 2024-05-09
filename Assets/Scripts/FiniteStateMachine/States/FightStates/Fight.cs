using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_FightState
{
    NotFight = 0,
    Attack = 1,
    StrongAttack = 2,
    Defense = 3,
    Dodge = 4,
}

public class Fight : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    public Weapon FightWeapon;

    public E_FightState nowFightState;
    
    public Fight(StateMachine stateMachine) : base(E_State.Fight, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _movementStateMachine.interactBox.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Enter();
        _movementStateMachine.interactBox.gameObject.SetActive(true);
        SetNowFightState(0);
    }

    // 动画事件-----
    public void SetNowFightState(int fightState)
    {
        nowFightState = (E_FightState)fightState;
        _movementStateMachine.playerAnimator.SetInteger(_movementStateMachine.DicAnimatorIndexes["NowFightState"],
            fightState);
    }
}
