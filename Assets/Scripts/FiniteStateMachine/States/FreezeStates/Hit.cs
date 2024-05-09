using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    public Vector3 hitPosition;
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
        if (preState.state == E_State.Fight)
        {
            _movementStateMachine.FightState.FightWeapon.ResetIsContinueAttack(0.3f);
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
        if (_timer >= 0.4f)
        {
            ExitHitState();
        }
    }

    private void CalculateHitDirectionAndToHit()
    {
        _directionToHit = (hitPosition - _movementStateMachine.playerTransform.position).normalized;
        // 点乘用来表示两个向量间的夹角，向量B在向量A上的投影长度，通过投影长度正负判断前后方位再通过角度判断左右
        // 在前方
        if (Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.forward) >= 0f)
        {
            if (Vector3.Angle(_directionToHit, _movementStateMachine.playerTransform.forward) <= 45f)
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(_movementStateMachine.DicAnimatorIndexes["ToHitBack"]);
                }
            }
            else if(Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.right) >= 0f)
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(
                        _movementStateMachine.DicAnimatorIndexes["ToHitRight"]);
                }
            }
            else
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(
                        _movementStateMachine.DicAnimatorIndexes["ToHitLeft"]);
                }
            }
        }
        // 后方
        else
        {
            if (Vector3.Angle(_directionToHit, -_movementStateMachine.playerTransform.forward) <= 45f)
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(
                        _movementStateMachine.DicAnimatorIndexes["ToHitFront"]);
                }
            }
            else if(Vector3.Dot(_directionToHit, _movementStateMachine.playerTransform.right) >= 0f)
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(
                        _movementStateMachine.DicAnimatorIndexes["ToHitRight"]);
                }
            }
            else
            {
                if (CheckCanPlayHitAnimation())
                {
                    _movementStateMachine.playerAnimator.SetTrigger(
                        _movementStateMachine.DicAnimatorIndexes["ToHitLeft"]);
                }
            }
        }
    }

    private bool CheckCanPlayHitAnimation()
    {
        return !(_movementStateMachine.FightState.FightWeapon != null &&
               _movementStateMachine.FightState.FightWeapon.isPlayTakeWeaponAnimation || 
               _movementStateMachine.FightState.FightWeapon != null &&
               _movementStateMachine.FightState.FightWeapon.isPlayUnTakeWeaponAnimation);
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
                else
                {
                    if (_movementStateMachine.CurrentState.state != E_State.Death)
                    {
                        _movementStateMachine.playerAnimator.SetTrigger(
                            _movementStateMachine.DicAnimatorIndexes["ToUnEquip"]);
                        _movementStateMachine.ChangeState(_movementStateMachine.IdleState);
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
    }
}
