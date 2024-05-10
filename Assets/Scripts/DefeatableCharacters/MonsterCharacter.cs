using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCharacter : DefeatableCharacter
{
    public MonsterStateMachine stateMachine;

    public void Start()
    {
        stateMachine = transform.GetComponent<MonsterStateMachine>();
    }

    public override bool Hit(float damage, Vector3 hitPosition, ICounterattack counterattack, bool isStrongAttack)
    {
        // 闪避状态
        if (stateMachine.CurrentState.state == E_State.Dodge ||
            stateMachine.CurrentState.state == E_State.Death)
        {
            return false;
        }
        
        
        // 完成攻击防御等状态之后完善这里
        
        
        // // 防御状态则对方攻击不造成击中效果
        // if (counterattack != null && stateMachine.CurrentState.state == E_State.Defense && !isStrongAttack))
        // {
        //     nowWeapon.isSuccessfulDefense = true;
        //     counterattack.Counterattack(0.1f,
        //         stateMachine.monsterCollider.ClosestPoint(hitPosition));
        //     return true;
        // }
        // 攻击状态弹刀
        // 攻击状态弹刀
        // if (counterattack != null && stateMachine.CurrentState.state == E_State.Attack && !isStrongAttack)
        // {
        //     stateMachine.HitState.
        //     PlayerMovementStateMachine.Instance.HitState.HitPosition = hitPosition;
        //     PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.HitState);
        //     counterattack.Counterattack(0.1f,
        //         PlayerMovementStateMachine.Instance.baseCollider.ClosestPoint(hitPosition));
        //     nowWeapon.Counterattack(0.1f,hitPosition);
        //     return true;
        // }

        stateMachine.HitState.HitPosition = hitPosition;
        stateMachine.ChangeState(stateMachine.HitState);
        return base.Hit(damage, hitPosition, counterattack, isStrongAttack);
    }

    public override void Death()
    {
        stateMachine.ChangeState(stateMachine.DeathState);
    }
}
