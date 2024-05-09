using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : DefeatableCharacter
{
    public Weapon nowWeapon;
    public override bool Hit(float damage, Vector3 hitPosition, ICounterattack counterattack, bool isStrongAttack)
    {
        // 闪避状态中击中失败
        if (PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Roll ||
            (PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Fight &&
             PlayerMovementStateMachine.Instance.FightState.nowFightState ==
             E_FightState.Dodge) ||
            PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Death)
        {
            return false;
        }
        
        // 防御状态则对方攻击不造成击中效果
        if (counterattack != null && nowWeapon.isDefensing && !isStrongAttack)
        {
            nowWeapon.isSuccessfulDefense = true;
            counterattack.Counterattack(0.1f,
                PlayerMovementStateMachine.Instance.baseCollider.ClosestPoint(hitPosition));
            return true;
        }

        // 攻击状态弹刀
        if (counterattack != null && PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Fight &&
            PlayerMovementStateMachine.Instance.FightState.nowFightState == E_FightState.Attack && !isStrongAttack)
        {
            PlayerMovementStateMachine.Instance.HitState.hitPosition = hitPosition;
            PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.HitState);
            counterattack.Counterattack(0.1f,
                PlayerMovementStateMachine.Instance.baseCollider.ClosestPoint(hitPosition));
            nowWeapon.Counterattack(0.1f,hitPosition);
            return true;
        }

        PlayerMovementStateMachine.Instance.HitState.hitPosition = hitPosition;
        PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.HitState);
        return base.Hit(damage, hitPosition, counterattack, isStrongAttack);
    }

    public override void Death()
    {
        PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.DeathState);
    }
}
