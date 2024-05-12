using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTurn : MonsterState
{
    public MonsterTurn(StateMachine stateMachine) : base(E_State.Turn, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        SelectTurn();
    }

    private void SelectTurn()
    {
        _monsterStateMachine.UpdateToTargetDirectionAndAngleImmediately();
        
        // 因为旋转动画旋转角度过小所以全部使用最大旋转角度
        if (Vector3.Dot(_monsterStateMachine.monsterToTargetDirection.normalized, _monsterStateMachine.transform.right) > 0f)
        {
            _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnRight180"]);
        }
        else
        {
            _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnLeft180"]);
        }
        
        // 正确角度
        // if (_monsterStateMachine.nowAngle < 135f)
        // {
        //     // 在右边
        //     if (Vector3.Dot(_monsterStateMachine.monsterToTargetDirection.normalized, _monsterStateMachine.transform.right) > 0f)
        //     {
        //         _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnRight90"]);
        //     }
        //     else
        //     {
        //         _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnLeft90"]);
        //     }
        // }
        // else
        // {
        //     // 在右边
        //     if (Vector3.Dot(_monsterStateMachine.monsterToTargetDirection.normalized, _monsterStateMachine.transform.right) > 0f)
        //     {
        //         _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnRight180"]);
        //     }
        //     else
        //     {
        //         _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToTurnLeft180"]);
        //     }
        // }
    }
}
