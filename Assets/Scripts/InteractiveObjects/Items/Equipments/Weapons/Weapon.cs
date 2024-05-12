using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum E_WeaponType
{
    Rod,
    Sword,
    Sickle,
}

public enum E_IsAllowRotate
{
    Yes = 0,
    No = 1,
}

public class Weapon : Equipment, ICounterattack
{
    public E_WeaponType weaponName;
    // 武器伤害
    public float weaponDamage;

    public List<string> listWeaponCanHitObjectTags; 
    
    public Transform weaponModelTransform;
    
    // 在地上作为物品时、放在背上装备中位置和旋转信息
    private Quaternion _weaponOnItemRotationOffset;
    // 背上
    public Vector3 weaponModelOnBackPositionOffset;
    public Quaternion weaponModelOnBackRotationOffset;
    
    // 是否正在进行防御
    public bool isDefensing;
    // 是否防御成功
    public bool isSuccessfulDefense;
    // 左键或右键摁下后继续监听另一个键摁下的延长时间(使更容易进入防御状态)
    public float extendTimeEasyToDefense;
    // 实际延长计时的具体时间
    public float extendTime;
    
    // 在判断继续进行攻击那一帧攻击键与瞄准键的输入情况记录
    private bool _fireInputOnContinueAttack;
    private bool _aimInputOnContinueAttack;
    
    // 是否正在播放拿武器动画
    public bool isPlayTakeWeaponAnimation;
    // 是否正在播放卸武器动画
    public bool isPlayUnTakeWeaponAnimation;
    
    // 是否改变过父节点
    private bool _isChangeModelFatherTransform;
    // 是否准备好了去攻击
    public bool isReadyToFight;
    // 攻击击中检测用变量
    private bool _isHit;
    // 计数用变量
    private int _count;
    // 战斗状态
    private Fight _fightState;
    // 是否继续攻击
    public bool isContinueAttack;
    // 动画播放时间计时器
    public float timer;
    // 动画播放时长计时器是否需要清零
    public bool timerIsToZero;
    // 当前攻击动画时长
    public float nowAttackAnimationTime;
    // 当前是否正在攻击
    public bool isAttacking;
    // 是否握住武器
    public bool isHoldWeapon;
    // 是否已经卸载武器
    public bool isReleaseWeapon;

    private PlayerCharacter _playerCharacter;
    private void Awake()
    {
        equipmentType = E_EquipmentType.Weapon;
        itemCollider = GetComponent<Collider>();

        _isChangeModelFatherTransform = false;
        _weaponOnItemRotationOffset = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isHit && isInUse)
        {
            for (_count = 0; _count < listWeaponCanHitObjectTags.Count; _count++)
            {
                if (other.CompareTag((listWeaponCanHitObjectTags[_count])))
                {
                    _isHit = other.transform.GetComponent<DefeatableCharacter>().Hit(weaponDamage,
                        itemCollider.ClosestPoint(other.transform.position), this,
                        controller.playerMovementStateMachine.FightState.nowFightState ==
                        E_FightState.StrongAttack);
                    if (_isHit)
                    {
                        controller.playerAnimator.speed = 0f;
                        Invoke(nameof(ResetAnimatorSpeed), 0.1f);
                    }
                    break;
                }
            }
        }
    }

    public override bool PickUpItem()
    {
        if (!(EquipmentsController.Instance is null))
        {
            controller = EquipmentsController.Instance;
            
            _fightState = controller.playerMovementStateMachine.FightState;
            _playerCharacter = controller.playerCharacter;
            
            weaponModelTransform.gameObject.SetActive(false);
            if(!(controller.AddEquipment(this)))
            {
                _fightState = null;
                _playerCharacter = null;
                weaponModelTransform.gameObject.SetActive(true);
                
                return false;
            }
        }
        else
        {
            return false;
        }
        
        tag = "Untagged";
        return true;
    }

    public override bool DiscardItem(Vector3 discardPosition)
    {
        // 设置父节点为场景
        transform.SetParent(null);
        weaponModelTransform.gameObject.SetActive(true);
        transform.rotation = Quaternion.identity;
        
        _fightState = null;
        _playerCharacter = null;
        _isChangeModelFatherTransform = false;

        tag = "Equipment"; 
        base.DiscardItem(discardPosition);
        transform.position += transform.up * 0.15f;
        transform.rotation = transform.rotation.normalized * _weaponOnItemRotationOffset;
        return true;
    }

    public override bool WearEquipment()
    {
        if (!_isChangeModelFatherTransform)
        {
            transform.SetParent(controller.weaponOnBackFatherTransform);
            transform.localPosition = weaponModelOnBackPositionOffset;
            transform.localRotation = weaponModelOnBackRotationOffset;
            
            _isChangeModelFatherTransform = true;
        }
        
        weaponModelTransform.gameObject.SetActive(true);
        _fightState.FightWeapon = this;
        _playerCharacter.nowWeapon = this;

        isInEquip = true;
        return true;
    }

    public override bool RemoveEquipment()
    {
        weaponModelTransform.gameObject.SetActive(false);
        
        isInEquip = false;

        _fightState.FightWeapon = null;
        _playerCharacter.nowWeapon = null;

        return true;
    }

    public override void ListenEquipmentUse()
    {
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["WeaponIsInUse"], isInUse);
        
        // 把武器装备到手上
        if (!isInUse &&
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput) &&
            controller.playerMovementStateMachine.CurrentState.state != E_State.Fight &&
            controller.playerMovementStateMachine.CurrentState.state != E_State.Death)
            // && setIsUseFalseTime == 0f)
        {
            isInUse = true;

            controller.playerAnimator.SetTrigger(
                controller.playerMovementStateMachine.DicAnimatorIndexes["ToEquip"]);
            
            ResetIsContinueAttack(2f);
            return;
        }

        // 武器已经拿到手上后摁开火键或瞄准键攻击
        if (isReadyToFight &&
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            if (!controller.playerMovementStateMachine.ChangeState(_fightState))
            {
                return;
            }
            
            isReadyToFight = false;
            controller.playerAnimator.SetTrigger(controller.playerMovementStateMachine.DicAnimatorIndexes["ReadyToFight"]);

            extendTime = 0f;
            isContinueAttack = true;
            controller.playerAnimator.SetBool(
                controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"], isContinueAttack);
            _fireInputOnContinueAttack = controller.equipmentUseInputInfo.FireInput;
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                _fireInputOnContinueAttack);
            _aimInputOnContinueAttack = controller.equipmentUseInputInfo.AimInput;
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                _aimInputOnContinueAttack);
            
            return;
        }

        if (timerIsToZero)
        {
            timer = 0f;
            timerIsToZero = false;
        }
        else if (nowAttackAnimationTime > float.Epsilon &&
                 controller.playerMovementStateMachine.CurrentState.state != E_State.Hit)
        {
            timer += Time.deltaTime * controller.playerMovementStateMachine.playerAnimator.speed;
        }


        // 检测是否继续进行攻击
        if (!isContinueAttack && 
            !isPlayUnTakeWeaponAnimation &&
            nowAttackAnimationTime > float.Epsilon &&
            timer <= nowAttackAnimationTime &&
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            extendTime = timer + extendTimeEasyToDefense;
            isContinueAttack = true;
            controller.playerAnimator.SetBool(
                controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"], isContinueAttack);
            _fireInputOnContinueAttack = controller.equipmentUseInputInfo.FireInput;
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                _fireInputOnContinueAttack);
            _aimInputOnContinueAttack = controller.equipmentUseInputInfo.AimInput;
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                _aimInputOnContinueAttack);
            return;
        }
        
        // 当时间超过了自动将状态由攻击状态转变为状态机中默认状态并将武器卸下
        if (nowAttackAnimationTime > float.Epsilon &&
            timer > nowAttackAnimationTime && !isDefensing)
        {
            if (controller.playerMovementStateMachine.CurrentState.state == E_State.Fight)
            {
                if (controller.playerMovementStateMachine.ChangeState(
                        controller.playerMovementStateMachine.GetInitialState()))
                {
                    isReadyToFight = true;
                    ResetIsContinueAttack(2f);
                }
            }
            else
            {
                ResetIsContinueAttack(0f);
                controller.playerAnimator.SetTrigger(controller.playerMovementStateMachine.DicAnimatorIndexes["ToUnEquip"]);
                isReadyToFight = false;
                return;
            }
        }
        
        // 延长监听便于进入防御状态
        if (isContinueAttack && nowAttackAnimationTime > float.Epsilon && timer < nowAttackAnimationTime &&
            timer < extendTime &&
            !(_fireInputOnContinueAttack && _aimInputOnContinueAttack) &&
            (controller.equipmentUseInputInfo.FireInput && !_fireInputOnContinueAttack ||
             controller.equipmentUseInputInfo.AimInput && !_aimInputOnContinueAttack))
        {
            if (!_fireInputOnContinueAttack)
            {
                _fireInputOnContinueAttack = true;
                controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                    _fireInputOnContinueAttack);
            }
            else
            {
                _aimInputOnContinueAttack = true;
                controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                    _aimInputOnContinueAttack);
            }

            extendTime = 0f;
        }

        if (timer >= nowAttackAnimationTime && isDefensing && isSuccessfulDefense)
        {
            isSuccessfulDefense = false;
            controller.playerAnimator.SetBool(
                controller.playerMovementStateMachine.DicAnimatorIndexes["IsSuccessfulDefense"], false);
        }
        
    }
    // 供对方成功防御后反击使用或同时处于攻击状态弹刀使用
    public void Counterattack(float damage, Vector3 hitPosition)
    {
        PlayerMovementStateMachine.Instance.HitState.HitPosition = hitPosition;
        PlayerMovementStateMachine.Instance.ChangeState(PlayerMovementStateMachine.Instance.HitState);
        _playerCharacter.hp -= damage;
        if (_playerCharacter.hp <= 0)
        {
            _playerCharacter.Death();
        }
    }
    // 恢复动画播放
    public void ResetAnimatorSpeed()
    {
        controller.playerAnimator.speed = 1f;
    }
    
    // 动画事件------
    // 开启攻击检测
    public void OpenCheckWeaponIsHit(int isAllowRotate)
    {
        isAttacking = true;
        InfoManager.Instance.isLockAttackDirection = isAllowRotate == (int)E_IsAllowRotate.Yes ? true : false;
        _isHit = false;
        itemCollider.enabled = true;
    }
    // 关闭攻击检测
    public void CloseCheckWeaponIsHit()
    {
        isAttacking = false;
        itemCollider.enabled = false;
        InfoManager.Instance.isLockAttackDirection = false;
    }
    // 重新开始监听是否摁了攻击键
    public void ResetIsContinueAttack(float time)
    {
        nowAttackAnimationTime = time;

        timerIsToZero = true;

        isDefensing = false;
        isSuccessfulDefense = false;
        controller.playerAnimator.SetBool(
            controller.playerMovementStateMachine.DicAnimatorIndexes["IsSuccessfulDefense"], isSuccessfulDefense);
        
        isContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"],
            false);
        _fireInputOnContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
            false);
        _aimInputOnContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
            false);
    }
    // 进入防御状态
    public void StartDefense()
    {
        isDefensing = true;
        isSuccessfulDefense = false;
        controller.playerAnimator.SetBool(
            controller.playerMovementStateMachine.DicAnimatorIndexes["IsSuccessfulDefense"], false);
    }
    // 停止防御状态
    public void EndDefense()
    {
        isDefensing = false;
    }
    
    // 装备武器抓住武器
    public void HoldWeapon()
    {
        isHoldWeapon = true;
        isReleaseWeapon = false;
        timerIsToZero = true;
        transform.SetParent(controller.weaponOnRightHandFatherTransform);
        controller.playerAnimator.SetInteger(controller.playerMovementStateMachine.DicAnimatorIndexes["WeaponName"], (int)weaponName);
    }
    // 卸下武器松开武器
    public void ReleaseWeapon(float time)
    {
        isHoldWeapon = false;
        isReleaseWeapon = true;
        transform.SetParent(controller.weaponOnBackFatherTransform);
        transform.localPosition = weaponModelOnBackPositionOffset;
        transform.localRotation = weaponModelOnBackRotationOffset;
        
        isContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"],
            false);
        _fireInputOnContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
            false);
        _aimInputOnContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
            false);
        timer = 0f; 
        isReadyToFight = false;
    }
    // 开始播放拿武器动画
    public void StartTakeWeaponAnimation()
    {
        isReadyToFight = false;
        isHoldWeapon = false;
        isPlayUnTakeWeaponAnimation = false;
    }
    // 拿武器动画结束
    public void EndTakeWeaponAnimation()
    {
        isPlayTakeWeaponAnimation = false;
        isReadyToFight = true;
    }
    // 开始播放卸武器动画
    public void StartUnTakeWeaponAnimation()
    {
        isPlayUnTakeWeaponAnimation = true;
        isReleaseWeapon = false;
    }
    // 卸武器动画结束播放
    public void EndUnTakeWeaponAnimation()
    {
        isPlayUnTakeWeaponAnimation = false;
        isInUse = false;
    }
}
