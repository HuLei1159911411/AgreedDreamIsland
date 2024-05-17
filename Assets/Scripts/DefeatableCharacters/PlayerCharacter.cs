using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerCharacter : DefeatableCharacter
{
    private static PlayerCharacter _instance;
    public static PlayerCharacter Instance => _instance;
    
    public Weapon nowWeapon;

    public float maxHp;
    public float maxStamina;
    public float stamina;
    public float staminaAddSpeed;
    public float climbAndWallRunningStaminaReduceSpeed;
    public float runStaminaReduceSpeed;
    public float changeStateReduceStaminaValue;
    public float hpAddSpeed;
    public bool _preFrameStaminaIsCanChangeState;
    public bool _nowFrameStaminaIsCanChangeState;

    public Action whenHpChange;
    public Action whenStaminaChange;

    private float _idleStateTimer;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        staminaAddSpeed *= Time.fixedDeltaTime;
        climbAndWallRunningStaminaReduceSpeed *= Time.fixedDeltaTime;
        runStaminaReduceSpeed *= Time.fixedDeltaTime;
        hpAddSpeed *= Time.fixedDeltaTime;
        
        ChangeHp(maxHp);
        stamina = maxStamina;
    }

    private void Start()
    {
        Init();
        PlayerMovementStateMachine.Instance.playerCharacter = this;
        PlayerMovementStateMachine.Instance.ChangeStateExternalJudgmentEvent +=
            CheckStaminaValueEnableCompleteChangeState;
    }

    private void FixedUpdate()
    {
        if (PlayerMovementStateMachine.Instance != null)
        {
            switch (PlayerMovementStateMachine.Instance.CurrentState.state)
            {
                case E_State.Idle :
                    if (stamina < maxStamina)
                    {
                        stamina += staminaAddSpeed * 2f;
                        if (stamina > maxStamina)
                        {
                            stamina = maxStamina;
                        }
                        whenStaminaChange?.Invoke();
                    }

                    if (_idleStateTimer >= 1.5f && hp < maxHp)
                    {
                        hp += hpAddSpeed;
                        if (hp > maxHp)
                        {
                            hp = maxHp;
                        }
                        whenHpChange?.Invoke();
                    }

                    
                    break;
                case E_State.Run :
                    stamina -= runStaminaReduceSpeed;
                    whenStaminaChange?.Invoke();
                    break;
                case E_State.WallRunning :
                    stamina -= climbAndWallRunningStaminaReduceSpeed;
                    whenStaminaChange?.Invoke();
                    break;
                case E_State.Climb :
                    stamina -= climbAndWallRunningStaminaReduceSpeed;
                    whenStaminaChange?.Invoke();
                    break;
                case E_State.Roll :
                    break;
                case E_State.Fall :
                    break;
                case E_State.Jump :
                    break;    
                case E_State.Fight :
                    break;
                case E_State.Sliding :
                    break;
                default:
                    if (stamina < maxStamina)
                    {
                        stamina += staminaAddSpeed;
                        if (stamina > maxStamina)
                        {
                            stamina = maxStamina;
                        }
                        whenStaminaChange?.Invoke();
                    }
                    break;
            }

            if (PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Idle)
            {
                if (_idleStateTimer < 1.5f)
                {
                    _idleStateTimer += Time.fixedDeltaTime;
                }
            }
            else
            {
                _idleStateTimer = 0f;
            }

            if (stamina >= changeStateReduceStaminaValue)
            {
                _nowFrameStaminaIsCanChangeState = true;
            }
            else
            {
                _nowFrameStaminaIsCanChangeState = false;
            }

            if (_preFrameStaminaIsCanChangeState != _nowFrameStaminaIsCanChangeState)
            {
                _preFrameStaminaIsCanChangeState = _nowFrameStaminaIsCanChangeState;
                PlayerMovementStateMachine.Instance.playerAnimator.SetBool(
                    PlayerMovementStateMachine.Instance.DicAnimatorIndexes["StaminaIsCanChangeState"],
                    _nowFrameStaminaIsCanChangeState);
            }
        }
    }

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
        if (counterattack != null && nowWeapon != null && nowWeapon.isDefensing && !isStrongAttack)
        {
            nowWeapon.isSuccessfulDefense = true;
            PlayerMovementStateMachine.Instance.playerAnimator.SetBool(
                PlayerMovementStateMachine.Instance.DicAnimatorIndexes["IsSuccessfulDefense"], true);
            counterattack.Counterattack(0.1f,
                PlayerMovementStateMachine.Instance.baseCollider.ClosestPoint(hitPosition));
            return true;
        }

        // 攻击状态弹刀
        if (counterattack != null && PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Fight &&
            PlayerMovementStateMachine.Instance.FightState.FightWeapon.isAttacking && !isStrongAttack)
        {
            counterattack.Counterattack(0.1f,
                PlayerMovementStateMachine.Instance.baseCollider.ClosestPoint(hitPosition));
            nowWeapon.Counterattack(0.1f,hitPosition);
            return true;
        }

        PlayerMovementStateMachine.Instance.HitState.HitPosition = hitPosition;
        PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.HitState);
        return base.Hit(damage, hitPosition, counterattack, isStrongAttack);
    }

    public override void Death()
    {
        PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.DeathState);
    }

    public bool CheckStaminaValueEnableCompleteChangeState(E_State newState)
    {
        switch (newState)
        {
            case E_State.Jump :
                // 在墙上跳消耗体力减75%
                if ((PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Climb ||
                     PlayerMovementStateMachine.Instance.CurrentState.state == E_State.WallRunning)
                    && stamina >= changeStateReduceStaminaValue * 0.25f)
                {
                    stamina -= changeStateReduceStaminaValue * 0.25f;
                    whenStaminaChange?.Invoke();
                    
                    if (stamina >= changeStateReduceStaminaValue)
                    {
                        _nowFrameStaminaIsCanChangeState = true;
                    }
                    else
                    {
                        _nowFrameStaminaIsCanChangeState = false;
                    }
                    if (_preFrameStaminaIsCanChangeState != _nowFrameStaminaIsCanChangeState)
                    {
                        _preFrameStaminaIsCanChangeState = _nowFrameStaminaIsCanChangeState;
                        PlayerMovementStateMachine.Instance.playerAnimator.SetBool(
                            PlayerMovementStateMachine.Instance.DicAnimatorIndexes["StaminaIsCanChangeState"],
                            _nowFrameStaminaIsCanChangeState);
                    }
                    
                    return true;
                }

                if (stamina >= changeStateReduceStaminaValue)
                {
                    stamina -= changeStateReduceStaminaValue;
                    whenStaminaChange?.Invoke();
                    
                    if (stamina >= changeStateReduceStaminaValue)
                    {
                        _nowFrameStaminaIsCanChangeState = true;
                    }
                    else
                    {
                        _nowFrameStaminaIsCanChangeState = false;
                    }
                    if (_preFrameStaminaIsCanChangeState != _nowFrameStaminaIsCanChangeState)
                    {
                        _preFrameStaminaIsCanChangeState = _nowFrameStaminaIsCanChangeState;
                        PlayerMovementStateMachine.Instance.playerAnimator.SetBool(
                            PlayerMovementStateMachine.Instance.DicAnimatorIndexes["StaminaIsCanChangeState"],
                            _nowFrameStaminaIsCanChangeState);
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
            case E_State.Roll :
                if (stamina >= changeStateReduceStaminaValue)
                {
                    stamina -= changeStateReduceStaminaValue;
                    whenStaminaChange?.Invoke();
                    
                    if (stamina >= changeStateReduceStaminaValue)
                    {
                        _nowFrameStaminaIsCanChangeState = true;
                    }
                    else
                    {
                        _nowFrameStaminaIsCanChangeState = false;
                    }
                    if (_preFrameStaminaIsCanChangeState != _nowFrameStaminaIsCanChangeState)
                    {
                        _preFrameStaminaIsCanChangeState = _nowFrameStaminaIsCanChangeState;
                        PlayerMovementStateMachine.Instance.playerAnimator.SetBool(
                            PlayerMovementStateMachine.Instance.DicAnimatorIndexes["StaminaIsCanChangeState"],
                            _nowFrameStaminaIsCanChangeState);
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
        }

        return true;
    }

    public override void ChangeHp(float value)
    {
        base.ChangeHp(value);
        if (hp < 0f)
        {
            hp = 0f;
        }

        if (hp > maxHp)
        {
            hp = maxHp;
        }
        
        whenHpChange?.Invoke();
    }

    public void Init()
    {
        ChangeHp(maxHp);
        stamina = maxStamina;
        _preFrameStaminaIsCanChangeState = false;
        _nowFrameStaminaIsCanChangeState = false;
    }
}
