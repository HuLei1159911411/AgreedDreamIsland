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

        _monsterStateMachine.runSoundEffect.Play();
        
        if (_monsterStateMachine.CheckNowAngle())
        {
            _monsterStateMachine.ChangeState(_monsterStateMachine.TurnState);
            return;
        }
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToRun"]);
    }

    public override void Exit()
    {
        base.Exit();
        
        _monsterStateMachine.runSoundEffect.Stop();
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
            if (_monsterStateMachine.monsterToTargetDistance <= 3f)
            {
                _monsterStateMachine.ChangeState(_monsterStateMachine.IdleState);
            }
        }
    }
}
