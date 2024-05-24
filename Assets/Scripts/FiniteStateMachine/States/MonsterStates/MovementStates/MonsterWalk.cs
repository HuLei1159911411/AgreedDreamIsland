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
        
        _monsterStateMachine.walkSoundEffect.Play();
        
        if (_monsterStateMachine.CheckNowAngle())
        {
            _monsterStateMachine.ChangeState(_monsterStateMachine.TurnState);
            return;
        }
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToWalk"]);
    }

    public override void Exit()
    {
        base.Exit();
        
        _monsterStateMachine.walkSoundEffect.Stop();
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        CheckStateChange();
    }

    private void CheckStateChange()
    {
        if (_monsterStateMachine.CheckPlayerDistanceAndAngleReadyToFight())
        {
            return;
        }
        
        if(_monsterStateMachine.nowAngle >= 15f)
        {
            if (_monsterStateMachine.CheckNowAngle())
            {
                _monsterStateMachine.ChangeState(_monsterStateMachine.TurnState);
                return;
            }
        }
        
        // 巡逻移动
        if (_monsterStateMachine.nowPatrolPointIndex != -1)
        {
            if (_monsterStateMachine.monsterToTargetDistance <= 1f)
            {
                _monsterStateMachine.ChangeState(_monsterStateMachine.IdleState);
            }
        }
    }
}
