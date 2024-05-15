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
    // 是否随机设置装备等级
    public bool isRandomSetEquipmentLevel;
    // 白色武器概率
    public float whiteLevelWeight;
    // 蓝色武器概率
    public float blueLevelWeight;
    // 紫色武器概率
    public float purpleLevelWeight;
    // 金色武器概率
    public float goldenLevelWeight;
    // 总概率
    private float _sumLevelWeight;
    // 是否通过武器等级设置武器伤害
    public bool isSetWeaponDamageByTypeAndLevel;
    // Rod类型武器伤害基础值
    public float rodTypeWeaponDamageValue;
    // Sword类型武器伤害基础值
    public float swordTypeWeaponDamageValue;
    // sickle类型武器伤害基础值
    public float sickleTypeWeaponDamageValue;
    // 白色等级武器伤害值倍率
    public float whiteLevelDamageValue;
    // 蓝色武器伤害值倍率
    public float blueLevelDamageValue;
    // 紫色武器伤害值倍率
    public float purpleLevelDamageValue;
    // 金色武器伤害值倍率
    public float goldenLevelDamageValue;
    // 武器伤害
    public float weaponDamage;

    public List<string> listWeaponCanHitObjectTags; 
    
    public Transform weaponModelTransform;
    public Transform weaponEffect;
    
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
    [HideInInspector] public bool isDefensing;
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
    // 是否停止武器所有行为
    public bool isStopWeaponAll;
    // 随机数
    private float _randomValue;
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
        
        transform.SetParent(null);
    }

    public void InitParams()
    {
        if (isRandomSetWeapon)
        {
            RandomSetWeaponTypeAndIndex();
        }

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
        
        _isChangeModelFatherTransform = false;
        _weaponOnItemRotationOffset = transform.rotation;
        isDefensing = false;
        isSuccessfulDefense = false;
        extendTime = 0f;
        _fireInputOnContinueAttack = false;
        _aimInputOnContinueAttack = false; 
        isPlayTakeWeaponAnimation = false;
        isPlayUnTakeWeaponAnimation = false;
        _isChangeModelFatherTransform = false;
        isReadyToFight = false;
        _isHit = false;
        isContinueAttack = false;
        timer = 0f; 
        timerIsToZero = false; 
        nowAttackAnimationTime = 0f;
        isAttacking = false;
        isHoldWeapon = false;
        isReleaseWeapon = false;
        toStartAttackTimer = 0f;
        isStopWeaponAll = false;
        equipmentName = (E_EquipmentName)(int)weaponType + 1;
        weaponEffect.gameObject.SetActive(false);
        _sumLevelWeight = whiteLevelWeight + blueLevelWeight + purpleLevelWeight + goldenLevelWeight;

        if (isRandomSetEquipmentLevel)
        {
            RandomSetEquipmentLevel();
        }

        if (isSetWeaponDamageByTypeAndLevel)
        {
            SetWeaponDamageByWeaponTypeAndEquipmentLevel();
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

        isAttacking = false;
        isDefensing = false;
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
    }

    private void RandomSetEquipmentLevel()
    {
        _randomValue = Random.Range(0, _sumLevelWeight);
        if (_randomValue < whiteLevelWeight)
        {
            equipmentLevel = E_EquipmentLevel.White;
            return;
        }

        _randomValue -= whiteLevelWeight;

        if (_randomValue < blueLevelWeight)
        {
            equipmentLevel = E_EquipmentLevel.Blue;
            return;
        }

        _randomValue -= blueLevelWeight;

        if (_randomValue < purpleLevelWeight)
        {
            equipmentLevel = E_EquipmentLevel.Purple;
            return;
        }

        equipmentLevel = E_EquipmentLevel.Golden;
    }

    private void SetWeaponDamageByWeaponTypeAndEquipmentLevel()
    {
        switch (weaponType)
        {
            case E_WeaponType.Rod:
                weaponDamage = rodTypeWeaponDamageValue;
                break;
            case E_WeaponType.Sword:
                weaponDamage = swordTypeWeaponDamageValue;
                break;
            case E_WeaponType.Sickle:
                weaponDamage = swordTypeWeaponDamageValue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (equipmentLevel)
        {
            case E_EquipmentLevel.White:
                weaponDamage *= whiteLevelDamageValue;
                break;
            case E_EquipmentLevel.Blue:
                weaponDamage *= blueLevelDamageValue;
                break;
            case E_EquipmentLevel.Purple:
                weaponDamage *= purpleLevelDamageValue;
                break;
            case E_EquipmentLevel.Golden:
                weaponDamage *= goldenLevelDamageValue;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        if (isStopWeaponAll)
        {
            return;
        }
        
        // 把武器装备到手上
        if (!isInUse &&
            (controller.EquipmentUseInputInfo.FireInput || controller.EquipmentUseInputInfo.AimInput) &&
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
            (controller.EquipmentUseInputInfo.FireInput || controller.EquipmentUseInputInfo.AimInput))
        {
            if (controller.EquipmentUseInputInfo.FireInput)
            {
                _fireInputOnContinueAttack = controller.EquipmentUseInputInfo.FireInput;
            }

            if (controller.EquipmentUseInputInfo.AimInput)
            {
                _aimInputOnContinueAttack = controller.EquipmentUseInputInfo.AimInput;
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
            (controller.EquipmentUseInputInfo.FireInput || controller.EquipmentUseInputInfo.AimInput))
        {
            extendTime = timer + extendTimeEasyToDefense;
            isContinueAttack = true;
            controller.playerAnimator.SetBool(
                controller.playerMovementStateMachine.DicAnimatorIndexes["IsContinueAttack"], isContinueAttack);
            _fireInputOnContinueAttack = controller.EquipmentUseInputInfo.FireInput;
            controller.playerAnimator.SetBool(controller.playerMovementStateMachine.DicAnimatorIndexes["FireInput"],
                _fireInputOnContinueAttack);
            _aimInputOnContinueAttack = controller.EquipmentUseInputInfo.AimInput;
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
                if (controller.playerMovementStateMachine.CurrentState.state != E_State.Death)
                {
                    controller.playerAnimator.SetTrigger(controller.playerMovementStateMachine.DicAnimatorIndexes["ToUnEquip"]);
                }
                isReadyToFight = false;
                return;
            }
        }
        
        // 延长监听便于进入防御状态
        if (isContinueAttack && nowAttackAnimationTime > float.Epsilon && timer < nowAttackAnimationTime &&
            timer < extendTime &&
            !(_fireInputOnContinueAttack && _aimInputOnContinueAttack) &&
            (controller.EquipmentUseInputInfo.FireInput && !_fireInputOnContinueAttack ||
             controller.EquipmentUseInputInfo.AimInput && !_aimInputOnContinueAttack))
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
        _playerCharacter.ChangeHp(-damage);
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
        weaponEffect.gameObject.SetActive(true);
        isAttacking = true;
        InfoManager.Instance.isLockAttackDirection = isAllowRotate == (int)E_IsAllowRotate.Yes ? true : false;
        _isHit = false;
        itemCollider.enabled = true;
    }
    // 关闭攻击检测
    public void CloseCheckWeaponIsHit()
    {
        weaponEffect.gameObject.SetActive(false);
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
        if (weaponEffect.gameObject.activeSelf)
        {
            weaponEffect.gameObject.SetActive(false);
        }
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
