using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public enum E_WeaponName
{
    Rod,
}

public enum E_IsAllowRotate
{
    Yes = 0,
    No = 1,
}

public class Weapon : Equipment
{
    public E_WeaponName weaponName;

    public List<string> listWeaponCanHitObjectTag; 
    
    public Transform weaponModelTransform;
    
    // 在地上作为物品时、放在背上装备中位置和旋转信息
    private Quaternion _weaponOnItemRotationOffset;
    // 背上
    public Vector3 weaponModelOnBackPositionOffset;
    public Quaternion weaponModelOnBackRotationOffset;

    // 是否改变过父节点
    private bool _isChangeModelFatherTransform;
    // 是否准备好了去攻击
    private bool _isReadyToFight;
    // 攻击击中检测用变量
    private bool _isHit;
    // 计数用变量
    private int _count;
    // 战斗状态
    private Fight _fightState;
    // 是否继续攻击
    private bool _isContinueAttack;
    // 动画播放时间计时器
    private float _timer;
    // 当前攻击动画时长
    private float _nowAttackAnimationTime;
    private void Awake()
    {
        equipmentType = E_EquipmentType.Weapon;
        itemCollider = GetComponent<Collider>();

        _isChangeModelFatherTransform = false;
        _weaponOnItemRotationOffset = transform.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isHit)
        {
            for (_count = 0; _count < listWeaponCanHitObjectTag.Count; _count++)
            {
                if (other.CompareTag((listWeaponCanHitObjectTag[_count])))
                {
                    // 攻击击中逻辑--------------------------------



                    _isHit = true;
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
            
            weaponModelTransform.gameObject.SetActive(false);
            if(!(controller.AddEquipment(this)))
            {
                _fightState = null;
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

    public override bool DiscardItem()
    {
        // 设置父节点为场景
        transform.SetParent(null);
        transform.localRotation = _weaponOnItemRotationOffset;

        _isChangeModelFatherTransform = false;

        tag = "Equipment";
        return base.DiscardItem();
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

        isInEquip = true;
        return true;
    }

    public override bool RemoveEquipment()
    {
        weaponModelTransform.gameObject.SetActive(false);
        
        isInEquip = false;

        _fightState.FightWeapon = null;

        return true;
    }

    public override void ListenEquipmentUse()
    {
        // 把武器装备到手上
        if (!isInUse && 
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            isInUse = true;

            controller.playerAnimator.SetTrigger(
                controller.playerMovementStateMachine.DicAnimatorIndexes["ToEquip"]);
            return;
        }

        // 武器已经拿到手上后摁左右键攻击
        if (_isReadyToFight && 
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            controller.playerAnimator.applyRootMotion = true;
            _isReadyToFight = false;
            _isContinueAttack = true;
            controller.playerAnimator.SetTrigger(controller.playerMovementStateMachine.DicAnimatorIndexes["ReadyToFight"]);
            controller.playerMovementStateMachine.ChangeState(_fightState);
            
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                controller.equipmentUseInputInfo.FireInput);
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                controller.equipmentUseInputInfo.AimInput);
            return;
        }

        if (!_isContinueAttack && _timer > 0.1f && _nowAttackAnimationTime > float.Epsilon &&
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            _isContinueAttack = true;
            controller.playerAnimator.SetBool(
                controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"], _isContinueAttack);
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                controller.equipmentUseInputInfo.FireInput);
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                controller.equipmentUseInputInfo.AimInput);
            return;
        }

        // 当时间超过了自动将状态由攻击状态转变为状态机中默认状态并将武器卸下
        if (!_isContinueAttack && _nowAttackAnimationTime > float.Epsilon && _timer < _nowAttackAnimationTime)
        {
            _timer += Time.deltaTime;
            if (_timer >= _nowAttackAnimationTime)
            {
                _nowAttackAnimationTime = 0f;
                controller.playerMovementStateMachine.ChangeState(
                    controller.playerMovementStateMachine.GetInitialState());
                controller.playerAnimator.SetTrigger(controller.playerMovementStateMachine.DicAnimatorIndexes["ToUnequip"]);
            }
        }
        
    }
    
    // 动画事件------
    // 装备武器抓住武器
    public void HoldWeapon()
    {
        _isReadyToFight = true;
        transform.SetParent(controller.weaponOnRightHandFatherTransform);
        controller.playerAnimator.SetInteger(controller.playerMovementStateMachine.DicAnimatorIndexes["WeaponName"], (int)weaponName);
    }
    // 卸下武器松开武器
    public void ReleaseWeapon()
    {
        transform.SetParent(controller.weaponOnBackFatherTransform);
        transform.localPosition = weaponModelOnBackPositionOffset;
        transform.localRotation = weaponModelOnBackRotationOffset;
        isInUse = false;
    }
    // 开启攻击检测
    public void OpenCheckWeaponIsHit(int isAllowRotate)
    {
        InfoManager.Instance.isLockAttackDirection = isAllowRotate == (int)E_IsAllowRotate.Yes ? true : false;
        _isHit = false;
        itemCollider.enabled = true;
    }
    // 关闭攻击检测
    public void CloseCheckWeaponIsHit()
    {
        itemCollider.enabled = false;
        InfoManager.Instance.isLockAttackDirection = false;
    }
    // 重新开始监听是否摁了攻击键
    public void ResetIsContinueAttack(float time)
    {
        _nowAttackAnimationTime = time;
        _timer = 0f;
        _isContinueAttack = false;
        controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"],
            false);
    }
}
