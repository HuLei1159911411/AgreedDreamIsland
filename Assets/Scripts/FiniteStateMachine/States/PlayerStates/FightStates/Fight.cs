using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_FightState
{
    NotFight = 0,
    Attack = 1,
    StrongAttack = 2,
    Defense = 3,
    Dodge = 4,
}

public class Fight : BaseState
{
    
    private PlayerMovementStateMachine _movementStateMachine;

    public Weapon FightWeapon;

    public E_FightState nowFightState;
    
    // 向前射线检测击中信息
    private RaycastHit _forwardRaycastHit;
    public Fight(StateMachine stateMachine) : base(E_State.Fight, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _movementStateMachine.interactBox.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Enter();
        _movementStateMachine.interactBox.gameObject.SetActive(true);
        SetNowFightState(0);
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        // 动画模型前方发射射线检测是否存在模型存在则将运动模式换位非根运动
        if (Physics.SphereCast(_movementStateMachine.playerTransform.position + _movementStateMachine.playerTransform.up * 0.5f, 
            0.5f,
                _movementStateMachine.playerTransform.forward,
                out _forwardRaycastHit,
                _movementStateMachine.baseCollider.radius + 0.5f, 
                InfoManager.Instance.layerGroundCheck))
        // Debug.Log("!_movementStateMachine.hasWallOnForward = " + !_movementStateMachine.hasWallOnForward + "\n" +
        //           "!_movementStateMachine.hasWallOnHeadForward = " + !_movementStateMachine.hasWallOnHeadForward + "\n" +
        //           "!_movementStateMachine.hasWallOnFootForward = " + !_movementStateMachine.hasWallOnFootForward);
        // if (!_movementStateMachine.hasWallOnForward && !_movementStateMachine.hasWallOnHeadForward &&
        //     !_movementStateMachine.hasWallOnFootForward)
        {
            if (_movementStateMachine.playerAnimator.applyRootMotion)
            {
                _movementStateMachine.playerAnimator.applyRootMotion = false;
            }
        }
        else
        {
            if (!_movementStateMachine.playerAnimator.applyRootMotion)
            {
                _movementStateMachine.playerAnimator.applyRootMotion = true;
            }
        }
    }

    // 动画事件-----
    public void SetNowFightState(int fightState)
    {
        nowFightState = (E_FightState)fightState;
        if (nowFightState != E_FightState.Defense && FightWeapon.isDefensing)
        {
            FightWeapon.isDefensing = false;
        }
        _movementStateMachine.playerAnimator.SetInteger(_movementStateMachine.DicAnimatorIndexes["NowFightState"],
            fightState);
    }
}
