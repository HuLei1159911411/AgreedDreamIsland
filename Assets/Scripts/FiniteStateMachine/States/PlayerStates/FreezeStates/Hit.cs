using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Hit : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    public Vector3 HitPosition;
    // 是否正在播放装备武器动画
    public bool isPlayTakeWeaponAnimation;
    // 是否正在播放卸载武器动画
    public bool isPlayUnTakeWeaponAnimation;
    // 到被击中点的方向
    private Vector3 _directionToHit;
    // 受击状态计时器
    private float _timer;
    public Hit(StateMachine stateMachine) : base(E_State.Hit, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _timer = 0f;
        if (_movementStateMachine.FightState.FightWeapon != null &&
            (_movementStateMachine.FightState.FightWeapon.isPlayTakeWeaponAnimation ||
             _movementStateMachine.FightState.FightWeapon.isPlayUnTakeWeaponAnimation))
        {
            isPlayTakeWeaponAnimation = _movementStateMachine.FightState.FightWeapon.isPlayTakeWeaponAnimation;
            isPlayUnTakeWeaponAnimation = _movementStateMachine.FightState.FightWeapon.isPlayUnTakeWeaponAnimation;
        }
        else
        {
            isPlayTakeWeaponAnimation = false;
            isPlayUnTakeWeaponAnimation = false;
        }
        if (preState.state == E_State.Fight)
        {
            _movementStateMachine.FightState.FightWeapon.ResetIsContinueAttack(0.8f);
            _movementStateMachine.FightState.FightWeapon.CloseCheckWeaponIsHit();
        }
        CalculateHitDirectionAndToHit();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _timer += Time.deltaTime;
        if (_timer >= 0.45f)
        {
            ExitHitState();
        }
    }

    // 计算击中方向设置播放击中动画
    private void CalculateHitDirectionAndToHit()
    {
        _directionToHit = (HitPosition - _movementStateMachine.playerTransform.position).normalized;
        // 点乘用来表示两个向量间的夹角，向量B在向量A上的投影长度，通过投影长度正负判断前后方位再通过角度判断左右
        // 在前方
        if (Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.forward) >= 0f)
        {
            if (Vector3.Angle(_directionToHit, _movementStateMachine.playerTransform.forward) <= 45f)
            {
                _movementStateMachine.playerAnimator.SetTrigger(_movementStateMachine.DicAnimatorIndexes["ToHitBack"]);
            }
            else if (Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.right) >= 0f)
            {
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToHitRight"]);
            }
            else
            {
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToHitLeft"]);
            }
        }
        // 后方
        else
        {
            if (Vector3.Angle(_directionToHit, -_movementStateMachine.playerTransform.forward) <= 45f)
            {
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToHitFront"]);
            }
            else if (Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.right) >= 0f)
            {
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToHitRight"]);
            }
            else
            {
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToHitLeft"]);
            }
        }
    }
    
    // 动画事件--------
    public void ExitHitState()
    {
        switch (preState.state)
        {
            case E_State.Fight :
                if (_movementStateMachine.FightState.FightWeapon.isContinueAttack)
                {
                    if (_movementStateMachine.CurrentState.state != E_State.Death)
                    {
                        _movementStateMachine.ChangeState(_movementStateMachine.FightState);
                    }
                }
                break;
                default:
                    if (_movementStateMachine.CurrentState.state != E_State.Death)
                    {
                        _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
                    }
                    break;
        }

        if (isPlayTakeWeaponAnimation)
        {
            if (_movementStateMachine.FightState.FightWeapon.isHoldWeapon)
            {
                _movementStateMachine.FightState.FightWeapon.isPlayTakeWeaponAnimation = false;
                _movementStateMachine.FightState.FightWeapon.isReadyToFight = true;
            }
            else
            {
                _movementStateMachine.FightState.FightWeapon.isInUse = true;
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToEquip"]);
                _movementStateMachine.FightState.FightWeapon.ResetIsContinueAttack(2f);
            }
        }

        if (isPlayUnTakeWeaponAnimation)
        {
            if (_movementStateMachine.FightState.FightWeapon.isReleaseWeapon)
            {
                _movementStateMachine.FightState.FightWeapon.isPlayUnTakeWeaponAnimation = false;
                _movementStateMachine.FightState.FightWeapon.isInUse = false;
            }
            else
            {
                _movementStateMachine.FightState.FightWeapon.timer = 0f;
                _movementStateMachine.playerAnimator.SetTrigger(
                    _movementStateMachine.DicAnimatorIndexes["ToUnEquip"]);
                _movementStateMachine.FightState.FightWeapon.isReadyToFight = false;
            }
        }
    }
}
