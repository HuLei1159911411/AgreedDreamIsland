using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorEvents : MonoBehaviour
{
    public  PlayerMovementStateMachine movementStateMachine;
    public EquipmentsController equipmentsController;

    public void ChangeStateClimbToFall()
    {
        movementStateMachine.ChangeStateClimbToFall();
    }

    public void ExitRollState()
    {
        movementStateMachine.ExitRollState();
    }

    public void InitGrappleHookTurnParameters()
    {
        movementStateMachine.InitGrappleHookTurnParameters();
    }
    
    // 拿起武器
    public void HoldWeapon()
    {
        equipmentsController.HoldWeapon();
    }
    // 放下武器
    public void ReleaseWeapon()
    {
        equipmentsController.ReleaseWeapon();
    }
    // 开始进行攻击击中检测
    public void OpenCheckWeaponIsHit(int isAllowRotate)
    {
        equipmentsController.OpenCheckWeaponIsHit(isAllowRotate);
    }
    // 结束攻击击中检测
    public void CloseCheckWeaponIsHit()
    {
        equipmentsController.CloseCheckWeaponIsHit();
    }
    // 重新开始监听是否摁了攻击键
    public void ResetIsContinueAttack(float time)
    {
        equipmentsController.ResetIsContinueAttack(time);
    }
}
