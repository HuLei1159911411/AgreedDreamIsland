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
    public E_WeaponType weaponType;
    public int weaponModelIndex;
    // 是否自动随机生成武器
    public bool isRandomSetWeapon;
    // 是否可以生成rod类型武器
    public bool canSetWeaponTypeRod;
    // 是否可以生成sword类型武器
    public bool canSetWeaponTypeSword;
    // 是否可以生成sickle类型武器
    public bool canSetWeaponTypeSickle;
    // 武器伤害
    public float weaponDamage;

    public List<string> listWeaponCanHitObjectTags; 
    
    public Transform weaponModelTransform;
    
    public Transform rodsFatherTransform;
    public Transform swordsFatherTransform;
    public Transform sicklesFatherTransform;
    
     public List<Transform> rods;
     public List<Transform> swords;
     public List<Transform> sickles;
    
    public Collider rodCollider;
    public Collider swordCollider;
    public Collider sickleCollider;
    
    // 在地上作为物品时、放在背上装备中位置和旋转信息
    private Quaternion _weaponOnItemRotationOffset;
    // 背上
    public Vector3 weaponModelOnBackPositionOffsetRod;
    public Quaternion weaponModelOnBackRotationOffsetRod;
    public Vector3 weaponModelOnBackPositionOffsetSword;
    public Quaternion weaponModelOnBackRotationOffsetSword;
    public Vector3 weaponModelOnBackPositionOffsetSickle;
    public Quaternion weaponModelOnBackRotationOffsetSickle;
    
    public Vector3 weaponModelOnBackPositionOffset;
    public Quaternion weaponModelOnBackRotationOffset;
    
    // 是否正在进行防御
  public bool isDefensing;
    // 是否防御成功
    [HideInInspector]  public bool isSuccessfulDefense;
    // 左键或右键摁下后继续监听另一个键摁下的延长时间(使更容易进入防御状态)
    public float extendTimeEasyToDefense;
    // 实际延长计时的具体时间
    [HideInInspector] public float extendTime;
    
    // 在判断继续进行攻击那一帧攻击键与瞄准键的输入情况记录
    private bool _fireInputOnContinueAttack;
    private bool _aimInputOnContinueAttack;
    
    // 是否正在播放拿武器动画
    [HideInInspector] public bool isPlayTakeWeaponAnimation;
    // 是否正在播放卸武器动画
    [HideInInspector] public bool isPlayUnTakeWeaponAnimation;
    
    // 是否改变过父节点
    private bool _isChangeModelFatherTransform;
    // 是否准备好了去攻击
    [HideInInspector] public bool isReadyToFight;
    // 攻击击中检测用变量
    private bool _isHit;
    // 计数用变量
    private int _count;
    // 战斗状态
    private Fight _fightState;
    // 是否继续攻击
    [HideInInspector] public bool isContinueAttack;
    // 动画播放时间计时器
    [HideInInspector] public float timer;
    // 动画播放时长计时器是否需要清零
    [HideInInspector] public bool timerIsToZero;
    // 当前攻击动画时长
    [HideInInspector] public float nowAttackAnimationTime;
    // 当前是否正在攻击
    [HideInInspector] public bool isAttacking;
    // 是否握住武器
    [HideInInspector] public bool isHoldWeapon;
    // 是否已经卸载武器
    [HideInInspector] public bool isReleaseWeapon;
    // 武器拿到手上开始攻击计时器
    [HideInInspector] public float toStartAttackTimer;

    private PlayerCharacter _playerCharacter;
    private void Awake()
    {
        AwakeInitParams();
        
        Init();
    }
    
    private void AwakeInitParams()
    {
        rods = new List<Transform>();
        for (_count = 0; _count < rodsFatherTransform.childCount; _count++)
        {
            rods.Add(rodsFatherTransform.GetChild(_count));
        }
        swords = new List<Transform>();
        for (_count = 0; _count < swordsFatherTransform.childCount; _count++)
        {
            swords.Add(swordsFatherTransform.GetChild(_count));
        }
        sickles = new List<Transform>();
        for (_count = 0; _count < sicklesFatherTransform.childCount; _count++)
        {
            sickles.Add(sicklesFatherTransform.GetChild(_count));
        }
    }

    public void Init()
    {
        InitParams();
    }

    public void InitParams()
    {
        if (isRandomSetWeapon)
        {
            RandomSetWeaponTypeAndIndex();
        }
        
        _isChangeModelFatherTransform = false;
        _weaponOnItemRotationOffset = transform.rotation;

        SetNowWeaponModel();

        SetWeaponCollider();

        switch (weaponType)
        {
            case E_WeaponType.Rod:
                strName = "棍";
                break;
            case E_WeaponType.Sword:
                strName = "剑";
                break;
            case E_WeaponType.Sickle:
                strName = "镰刀";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (weaponType)
        {
            case E_WeaponType.Rod:
                weaponModelOnBackPositionOffset = weaponModelOnBackPositionOffsetRod;
                weaponModelOnBackRotationOffset = weaponModelOnBackRotationOffsetRod;
                break;
            case E_WeaponType.Sword:
                weaponModelOnBackPositionOffset = weaponModelOnBackPositionOffsetSword;
                weaponModelOnBackRotationOffset = weaponModelOnBackRotationOffsetSword;
                break;
            case E_WeaponType.Sickle:
                weaponModelOnBackPositionOffset = weaponModelOnBackPositionOffsetSickle;
                weaponModelOnBackRotationOffset = weaponModelOnBackRotationOffsetSickle;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetNowWeaponModel()
    {
        switch (weaponType)
        {
            case E_WeaponType.Rod:
                rodsFatherTransform.gameObject.SetActive(true);
                swordsFatherTransform.gameObject.SetActive(false);
                sicklesFatherTransform.gameObject.SetActive(false);
                for (_count = 0; _count < rods.Count; _count++)
                {
                    if (_count == weaponModelIndex)
                    {
                        rods[_count].gameObject.SetActive(true);
                        weaponModelTransform = rods[_count];
                    }
                    else
                    {
                        rods[_count].gameObject.SetActive(false);
                    }
                }
                break;
            case E_WeaponType.Sword:
                rodsFatherTransform.gameObject.SetActive(false);
                swordsFatherTransform.gameObject.SetActive(true);
                sicklesFatherTransform.gameObject.SetActive(false);
                for (_count = 0; _count < swords.Count; _count++)
                {
                    if (_count == weaponModelIndex)
                    {
                        swords[_count].gameObject.SetActive(true);
                        weaponModelTransform = swords[_count];
                    }
                    else
                    {
                        swords[_count].gameObject.SetActive(false);
                    }
                }
                break;
            case E_WeaponType.Sickle:
                rodsFatherTransform.gameObject.SetActive(false);
                swordsFatherTransform.gameObject.SetActive(false);
                sicklesFatherTransform.gameObject.SetActive(true);
                for (_count = 0; _count < sickles.Count; _count++)
                {
                    if (_count == weaponModelIndex)
                    {
                        sickles[_count].gameObject.SetActive(true);
                        weaponModelTransform = sickles[_count];
                    }
                    else
                    {
                        sickles[_count].gameObject.SetActive(false);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetWeaponCollider()
    {
        switch (weaponType)
        {
            case E_WeaponType.Rod:
                itemCollider = rodCollider;
                
                swordCollider.enabled = false;
                sickleCollider.enabled = false;
                
                itemCollider.enabled = true;
                break;
            case E_WeaponType.Sword:
                itemCollider = swordCollider;
                
                rodCollider.enabled = false;
                sickleCollider.enabled = false;
                
                itemCollider.enabled = true;
                break;
            case E_WeaponType.Sickle:
                itemCollider = sickleCollider;
                
                rodCollider.enabled = false;
                swordCollider.enabled = false;
                
                itemCollider.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 随机设置武器类型与索引
    public void RandomSetWeaponTypeAndIndex()
    {
        weaponType = E_WeaponType.Rod;
        if (canSetWeaponTypeRod && canSetWeaponTypeSword && canSetWeaponTypeSickle)
        {
            weaponType = (E_WeaponType)Random.Range(0, 3);
        }
        else if(canSetWeaponTypeRod && canSetWeaponTypeSword)
        {
            weaponType = (E_WeaponType)Random.Range(0, 2);
        }
        else if(canSetWeaponTypeSword && canSetWeaponTypeSickle)
        {
            weaponType = (E_WeaponType)Random.Range(1, 3);
        }
        else if(canSetWeaponTypeRod && canSetWeaponTypeSickle)
        {
            weaponType = Random.Range(0, 2) == 0 ? E_WeaponType.Rod : E_WeaponType.Sickle;
        }
        else
        {
            if (canSetWeaponTypeRod)
            {
                weaponType = E_WeaponType.Rod;
            }
            
            if (canSetWeaponTypeSword)
            {
                weaponType = E_WeaponType.Sword;
            }

            if (canSetWeaponTypeSickle)
            {
                weaponType = E_WeaponType.Sickle;
            }
        }
        
        switch (weaponType)
        {
            case E_WeaponType.Rod:
                weaponModelIndex = Random.Range(0, rods.Count);
                break;
            case E_WeaponType.Sword:
                weaponModelIndex = Random.Range(0, swords.Count);
                break;
            case E_WeaponType.Sickle:
                weaponModelIndex = Random.Range(0, sickles.Count);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        equipmentName = (E_EquipmentName)(int)equipmentType + 1;
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

            _fireInputOnContinueAttack = false;
            _aimInputOnContinueAttack = false;
            
            ResetIsContinueAttack(2f);
            return;
        }

        if (isReadyToFight && (_fireInputOnContinueAttack || _aimInputOnContinueAttack))
        {
            toStartAttackTimer += Time.deltaTime;
        }
        if (toStartAttackTimer >= 0.1f)
        {
            toStartAttackTimer = 0f;
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
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                _fireInputOnContinueAttack);
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["AimInput"],
                _aimInputOnContinueAttack);
        }

        // 武器已经拿到手上后摁开火键或瞄准键攻击
        if (isReadyToFight &&
            (controller.equipmentUseInputInfo.FireInput || controller.equipmentUseInputInfo.AimInput))
        {
            if (controller.equipmentUseInputInfo.FireInput)
            {
                _fireInputOnContinueAttack = controller.equipmentUseInputInfo.FireInput;
            }

            if (controller.equipmentUseInputInfo.AimInput)
            {
                _aimInputOnContinueAttack = controller.equipmentUseInputInfo.AimInput;
            }
            
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
        controller.playerAnimator.SetInteger(controller.playerMovementStateMachine.DicAnimatorIndexes["WeaponType"], (int)weaponType);
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
