using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterWeapon : MonoBehaviour, ICounterattack
{
    public MonsterStateMachine monsterStateMachine;
    public MonsterCharacter monsterCharacter;

    public Vector3 monsterWeaponEquipLocalPosition;
    public Quaternion monsterWeaponEquipLocalRotation;

    public Transform rodsFatherTransform;
    public Transform swordsFatherTransform;
    public Transform sicklesFatherTransform;

    public List<Transform> rods;
    public List<Transform> swords;
    public List<Transform> sickles;

    public Collider rodCollider;
    public Collider swordCollider;
    public Collider sickleCollider;
    
    // 是否自动随机设置武器类型
    public bool isRandomSetWeaponType;
    // 是否可以生成rod类型武器
    public bool canSetWeaponTypeRod;
    // 是否可以生成sword类型武器
    public bool canSetWeaponTypeSword;
    // 是否可以生成sickle类型武器
    public bool canSetWeaponTypeSickle;
    // 当前武器类型
    public E_WeaponType nowWeaponType;
    // 是否自动随机设置武器索引
    public bool isRandomSetWeaponIndex;
    // 当前武器索引值
    public int nowWeaponIndex;
    // 当前武器伤害
    public float weaponDamage;
    // 当前武器碰撞盒
    public Collider nowCollider;
    // 是否击中
    public bool isHit;
    // 可被击中的所有对象的Tag
    public List<string> listCanHitObjectTags;
    // 是否正在攻击
    public bool isAttacking;

    private int _count;
    public bool isAwakeInit;

    public void Start()
    {
        Init();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!isHit)
        {
            for (_count = 0; _count < listCanHitObjectTags.Count; _count++)
            {
                if (other.CompareTag((listCanHitObjectTags[_count])))
                {
                    isHit = other.transform.parent.parent.GetComponent<DefeatableCharacter>().Hit(weaponDamage,
                        nowCollider.ClosestPoint(other.transform.position), this,
                        monsterStateMachine.AttackState.AttackType ==
                        E_AttackType.StrongAttack);
                    if (isHit)
                    {
                        monsterStateMachine.animator.speed = 0f;
                        Invoke(nameof(ResetAnimatorSpeed), 0.2f);
                    }
                    break;
                }
            }
        }
    }

    private void AwakeInitParams()
    {
        isAwakeInit = true;
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
        if (!isAwakeInit)
        {
            AwakeInitParams();
        }
        
        InitParams();
        
        if (monsterStateMachine != null)
        {
            transform.SetParent(monsterStateMachine.weaponEquipFatherTransform);
            transform.localPosition = monsterWeaponEquipLocalPosition;
            transform.localRotation = monsterWeaponEquipLocalRotation;
            transform.localScale = Vector3.one;
            monsterCharacter = monsterStateMachine.monsterCharacter;
        }
    }

    public void InitParams()
    {
        if (isRandomSetWeaponType)
        {
            RandomSetWeaponType();
        }

        if (isRandomSetWeaponIndex)
        {
            RandomSetWeaponIndex();
        }

        SetNowWeaponModel();

        SetWeaponCollider();
    }

    // 随机设置武器类型
    public void RandomSetWeaponType()
    {
        nowWeaponType = E_WeaponType.Rod;
        if (canSetWeaponTypeRod && canSetWeaponTypeSword && canSetWeaponTypeSickle)
        {
            nowWeaponType = (E_WeaponType)Random.Range(0, 3);
        }
        else if(canSetWeaponTypeRod && canSetWeaponTypeSword)
        {
            nowWeaponType = (E_WeaponType)Random.Range(0, 2);
        }
        else if(canSetWeaponTypeSword && canSetWeaponTypeSickle)
        {
            nowWeaponType = (E_WeaponType)Random.Range(1, 3);
        }
        else if(canSetWeaponTypeRod && canSetWeaponTypeSickle)
        {
            nowWeaponType = Random.Range(0, 2) == 0 ? E_WeaponType.Rod : E_WeaponType.Sickle;
        }
        else
        {
            if (canSetWeaponTypeRod)
            {
                nowWeaponType = E_WeaponType.Rod;
            }
            
            if (canSetWeaponTypeSword)
            {
                nowWeaponType = E_WeaponType.Sword;
            }

            if (canSetWeaponTypeSickle)
            {
                nowWeaponType = E_WeaponType.Sickle;
            }
        }
    }

    private void RandomSetWeaponIndex()
    {
        switch (nowWeaponType)
        {
            case E_WeaponType.Rod:
                nowWeaponIndex = Random.Range(0, rods.Count);
                break;
            case E_WeaponType.Sword:
                nowWeaponIndex = Random.Range(0, swords.Count);
                break;
            case E_WeaponType.Sickle:
                nowWeaponIndex = Random.Range(0, sickles.Count);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 设置当前武器模型通过nowWeaponType和nowWeaponIndex
    public void SetNowWeaponModel()
    {
        switch (nowWeaponType)
        {
            case E_WeaponType.Rod:
                rodsFatherTransform.gameObject.SetActive(true);
                swordsFatherTransform.gameObject.SetActive(false);
                sicklesFatherTransform.gameObject.SetActive(false);
                for (_count = 0; _count < rods.Count; _count++)
                {
                    if (_count == nowWeaponIndex)
                    {
                        rods[_count].gameObject.SetActive(true);
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
                    if (_count == nowWeaponIndex)
                    {
                        swords[_count].gameObject.SetActive(true);
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
                    if (_count == nowWeaponIndex)
                    {
                        sickles[_count].gameObject.SetActive(true);
                    }
                    else
                    {
                        sickles[_count].gameObject.SetActive(false);
                    }
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(nowWeaponType), nowWeaponType, null);
        }
    }
    // 设置武器碰撞盒
    private void SetWeaponCollider()
    {
        switch (nowWeaponType)
        {
            case E_WeaponType.Rod:
                nowCollider = rodCollider;
                
                rodCollider.enabled = true;
                swordCollider.enabled = false;
                sickleCollider.enabled = false;
                
                nowCollider.enabled = false;
                break;
            case E_WeaponType.Sword:
                nowCollider = swordCollider;
                
                rodCollider.enabled = false;
                swordCollider.enabled = true;
                sickleCollider.enabled = false;
                
                nowCollider.enabled = false;
                break;
            case E_WeaponType.Sickle:
                nowCollider = sickleCollider;
                
                rodCollider.enabled = false;
                swordCollider.enabled = false;
                sickleCollider.enabled = true;
                
                nowCollider.enabled = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    // 恢复动画机速度
    private void ResetAnimatorSpeed()
    {
        monsterStateMachine.animator.speed = 1f;
    }

    // 对方进行反击
    public void Counterattack(float damage, Vector3 hitPosition)
    {
        // 闪避状态
        if (monsterStateMachine.CurrentState.state == E_State.Death)
        {
            return;
        }
        
        monsterStateMachine.HitState.HitPosition = hitPosition;
        
        monsterCharacter.hp -= damage;
        monsterStateMachine.ChangeState(monsterStateMachine.HitState);
    }
}
