using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterIdle : MonsterState
{
    // 在idle状态计时器
    public float IdleTimer;
    public MonsterIdle(StateMachine stateMachine) : base(E_State.Idle, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        IdleTimer = 0f;
        _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToIdle"]);
    }

    public override void UpdatePhysic()
    {
        IdleTimer += Time.fixedDeltaTime;

        if (IdleTimer >= _monsterStateMachine.maxIdleTime)
        {
            // 在存在巡逻点时
            if (_monsterStateMachine.isHasPatrolPoints)
            {
                // 尝试换成奔跑巡逻、走路巡逻、或继续站着不动状态
                switch (Random.Range(0, 3))
                {
                    case 0 :
                        break;
                    case 1 :
                        _monsterStateMachine.nowTarget = _monsterStateMachine.RandomGetPatrolPoint();
                        _monsterStateMachine.ChangeState(_monsterStateMachine.WalkState);
                        break;
                    case 2 :
                        _monsterStateMachine.nowTarget = _monsterStateMachine.RandomGetPatrolPoint();
                        _monsterStateMachine.ChangeState(_monsterStateMachine.RunState);
                        break;
                }

                IdleTimer = 0f;
            }
        }
    }
}
