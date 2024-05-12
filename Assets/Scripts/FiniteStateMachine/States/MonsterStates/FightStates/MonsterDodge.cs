using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDodge : MonsterState
{
    private int _randomValueOnSelectDodgeAnimation;
    public MonsterDodge(StateMachine stateMachine) : base(E_State.Dodge, stateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        RandomSelectDodgeAnimation();
    }

    // 随机选择闪避方向并进行闪避
    private void RandomSelectDodgeAnimation()
    {
        _randomValueOnSelectDodgeAnimation = Random.Range(0, 4);
        switch (_randomValueOnSelectDodgeAnimation)
        {
            case 0 :
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDodgeFront"]);
                break;
            case 1 :
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDodgeBack"]);
                break;
            case 2 :
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDodgeLeft"]);
                break;
            case 3 :
                _monsterStateMachine.animator.SetTrigger(_monsterStateMachine.DicAnimatorIndexes["ToDodgeRight"]);
                break;
        }
    }
}
