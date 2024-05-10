using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterState : BaseState
{
    protected MonsterStateMachine _monsterStateMachine;
    public MonsterState(E_State state, StateMachine stateMachine) : base(state, stateMachine)
    {
        if (stateMachine is MonsterStateMachine)
        {
            _monsterStateMachine = stateMachine as MonsterStateMachine;
        }
    }
}
