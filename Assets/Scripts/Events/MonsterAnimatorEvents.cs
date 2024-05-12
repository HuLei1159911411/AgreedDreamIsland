using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimatorEvents : MonoBehaviour
{
    public MonsterStateMachine monsterStateMachine;

    public void EndAnimation()
    {
        monsterStateMachine.ChooseAndChangeNextState();
    }

    public void StartAttack()
    {
        monsterStateMachine.StartAttack();
    }

    public void EndAttack()
    {
        monsterStateMachine.EndAttack();
    }

    public void StartDefense()
    {
        monsterStateMachine.StartDefense();
    }

    public void EndDefense()
    {
        monsterStateMachine.EndDefense();
    }
}
