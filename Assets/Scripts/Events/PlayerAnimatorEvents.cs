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
    public void ReleaseWeapon(float time)
    {
        equipmentsController.ReleaseWeapon(time);
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
    
    // 开始防御
    public void StartDefense()
    {
        equipmentsController.StartDefense();
    }
    // 结束防御
    public void EndDefense()
    {
        equipmentsController.EndDefense();
    }
    // 设置当前战斗状态
    public void SetNowFightState(int fightState)
    {
        movementStateMachine.SetNowFightState(fightState);
    }
    // 退出受击状态
    public void ExitHitState()
    {
        movementStateMachine.ExitHitState();
    }
    // 开始播放拿武器动画
    public void StartTakeWeaponAnimation()
    {
        equipmentsController.StartTakeWeaponAnimation();
    }
    // 结束拿武器动画
    public void EndTakeWeaponAnimation()
    {
        equipmentsController.EndTakeWeaponAnimation();
    }
    // 开始播放卸武器动画
    public void StartUnTakeWeaponAnimation()
    {
        equipmentsController.StartUnTakeWeaponAnimation();
    }
    // 结束播放卸武器动画
    public void EndUnTakeWeaponAnimation()
    {
        equipmentsController.EndUnTakeWeaponAnimation();
    }
    // 战斗翻滚减体力
    public void ReduceStamina()
    {
        movementStateMachine.ReduceStamina();
    }
}
