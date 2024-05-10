using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterWeapon : MonoBehaviour
{
    public MonsterStateMachine monsterStateMachine;

    public Vector3 monsterWeaponEquipLocalPosition;
    public Quaternion monsterWeaponEquipLocalRotation;

    public Transform rodsFatherTransform;
    public Transform swordsFatherTransform;
    public Transform sicklesFatherTransform;

    public Transform[] rods;
    public Transform[] swords;
    public Transform[] sickles;

    public Collider rodCollider;
    public Collider swordCollider;
    public Collider sickleCollider;
    
    // 是否自动随机生成武器
    public bool isRandomSetWeapon;
    // 是否可以生成rod类型武器
    public bool canSetWeaponTypeRod;
    // 是否可以生成sword类型武器
    public bool canSetWeaponTypeSword;
    // 是否可以生成sickle类型武器
    public bool canSetWeaponTypeSickle;
    // 当前武器类型
    public E_WeaponType nowWeaponType;
    // 当前武器索引值
    public int nowWeaponIndex;

    private int _count;

    public void Awake()
    {
        AwakeInitParams();
        Init();
    }

    public void Start()
    {
        if (monsterStateMachine != null)
        {
            transform.SetParent(monsterStateMachine.weaponEquipFatherTransform);
            transform.localPosition = monsterWeaponEquipLocalPosition;
            transform.localRotation = monsterWeaponEquipLocalRotation;
            transform.localScale = Vector3.one;
        }
    }

    private void AwakeInitParams()
    {
        rods = new Transform[rodsFatherTransform.childCount];
        for (_count = 0; _count < rods.Length; _count++)
        {
            rods[_count] = rodsFatherTransform.GetChild(_count);
        }
        swords = new Transform[swordsFatherTransform.childCount];
        for (_count = 0; _count < swords.Length; _count++)
        {
            swords[_count] = swordsFatherTransform.GetChild(_count);
        }
        sickles = new Transform[sicklesFatherTransform.childCount];
        for (_count = 0; _count < sickles.Length; _count++)
        {
            sickles[_count] = sicklesFatherTransform.GetChild(_count);
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

        SetNowWeaponModel();

        SetWeaponCollider();
    }

    // 随机设置武器类型与索引
    public void RandomSetWeaponTypeAndIndex()
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
        
        switch (nowWeaponType)
        {
            case E_WeaponType.Rod:
                nowWeaponIndex = Random.Range(0, rods.Length);
                break;
            case E_WeaponType.Sword:
                nowWeaponIndex = Random.Range(0, swords.Length);
                break;
            case E_WeaponType.Sickle:
                nowWeaponIndex = Random.Range(0, sickles.Length);
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
                for (_count = 0; _count < rods.Length; _count++)
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
                for (_count = 0; _count < swords.Length; _count++)
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
                for (_count = 0; _count < sickles.Length; _count++)
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
                rodCollider.enabled = true;
                swordCollider.enabled = false;
                sickleCollider.enabled = false;
                break;
            case E_WeaponType.Sword:
                rodCollider.enabled = false;
                swordCollider.enabled = true;
                sickleCollider.enabled = false;
                break;
            case E_WeaponType.Sickle:
                rodCollider.enabled = false;
                swordCollider.enabled = false;
                sickleCollider.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
