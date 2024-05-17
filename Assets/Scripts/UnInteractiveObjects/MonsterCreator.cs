using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterCreator : MonoBehaviour
{
    [Header("怪物生成器信息")] 
    public int monsterMaxSum;
    public int monsterNowSum;
    public int nowMaxMonsterCount;
    public int nowMonsterCount;
    public float monsterBuildCoolDownTime;
    [Header("怪物生成信息")] 
    public Vector3 monsterScale;
    public float monsterHp;
    public int monsterIndex;
    public bool isRandomCreateMonster;
    // 随机设置怪物武器的索引值为指定索引值中其中值(若该容器为null或容器内元素数量为0则该类型武器所有索引均会随机到)
    public List<int> listRandomCreateMonsterIndexes;

    [Header("怪物武器生成信息")]
    // 怪物武器伤害
    public float weaponDamage;
    // 怪物武器类型
    public E_WeaponType monsterWeaponType;
    // 是否随机设置怪物武器类型
    public bool isRandomSetMonsterWeaponType;
    // 随机设置时是否可以生成rod类型武器
    public bool canSetWeaponTypeRod;
    // 随机设置时是否可以生成sword类型武器
    public bool canSetWeaponTypeSword;
    // 随机设置时是否可以生成sickle类型武器
    public bool canSetWeaponTypeSickle;
    // 怪物武器索引
    public int monsterWeaponIndex;
    // 是否随机设置怪物武器索引值
    public bool isRandomSetMonsterWeaponIndex;
    // 随机设置怪物的索引值为指定索引值中其中的值(若该容器为null或容器内元素数量为0则该类型武器所有索引均会随机到)
    public List<int> listRandomSetWeaponIndexes;

    public Transform patrolPointsFatherTransform;
    public List<Transform> listPatrolPoints;
    private float _timer;
    private int _count;

    private GameObject _nowMonsterObject;
    private MonsterStateMachine _nowMonsterStateMachine;
    private GameObject _nowMonsterWeaponObject;
    private MonsterWeapon _nowMonsterWeapon;
    private GameObject _nowMonsterHpObject;
    private MonsterHpController _nowMonsterHpController;
    private void Awake()
    {
        if (patrolPointsFatherTransform != null)
        {
            listPatrolPoints = new List<Transform>();
            for (_count = 0; _count < patrolPointsFatherTransform.childCount; _count++)
            {
                listPatrolPoints.Add(patrolPointsFatherTransform.GetChild(_count));
            }
        }

        if (monsterScale == Vector3.zero)
        {
            monsterScale = Vector3.one;
        }

        if (monsterHp <= float.Epsilon)
        {
            monsterHp = 10f;
        }

        if (weaponDamage <= float.Epsilon)
        {
            weaponDamage = 1f;
        }
    }

    private void Update()
    {
        if (monsterNowSum == monsterMaxSum && monsterMaxSum != -1)
        {
            return;
        }
        
        if (nowMonsterCount < nowMaxMonsterCount)
        {
            _timer += Time.deltaTime;
        }

        if (_timer >= monsterBuildCoolDownTime)
        {
            _timer = 0f;
            StartCoroutine(CreateMonsterAndMonsterWeapon());
        }
    }

    private IEnumerator CreateMonsterAndMonsterWeapon()
    {
        nowMonsterCount++;
        monsterNowSum++;
        _nowMonsterObject = ObjectPoolManager.Instance.GetObject(E_ObjectType.Monster);
        _nowMonsterObject.SetActive(false);
        _nowMonsterObject.transform.position = transform.position;
        _nowMonsterObject.transform.rotation = transform.rotation;
        _nowMonsterObject.transform.localScale = monsterScale;
        _nowMonsterStateMachine = _nowMonsterObject.GetComponent<MonsterStateMachine>();

        _nowMonsterStateMachine.monsterCreator = this;
        
        _nowMonsterStateMachine.listPatrolPoints = listPatrolPoints;
        if (isRandomCreateMonster)
        {
            if (listRandomCreateMonsterIndexes == null || listRandomCreateMonsterIndexes.Count == 0)
            {
                _nowMonsterStateMachine.monsterCharacter.nowMonsterModelIndex = -1;
            }
            else
            {
                _nowMonsterStateMachine.monsterCharacter.nowMonsterModelIndex =
                    listRandomCreateMonsterIndexes[Random.Range(0, listRandomCreateMonsterIndexes.Count)];
            }
        }
        else
        {
            _nowMonsterStateMachine.monsterCharacter.nowMonsterModelIndex = monsterIndex;
        }
        
        _nowMonsterWeaponObject = ObjectPoolManager.Instance.GetObject(E_ObjectType.MonsterWeapon);
        _nowMonsterWeaponObject.SetActive(false);
        _nowMonsterWeapon = _nowMonsterWeaponObject.GetComponent<MonsterWeapon>();
        _nowMonsterWeapon.isRandomSetWeaponType = isRandomSetMonsterWeaponType;
        _nowMonsterWeapon.canSetWeaponTypeRod = canSetWeaponTypeRod;
        _nowMonsterWeapon.canSetWeaponTypeSword = canSetWeaponTypeSword;
        _nowMonsterWeapon.canSetWeaponTypeSickle = canSetWeaponTypeSickle;
        _nowMonsterWeapon.nowWeaponType = monsterWeaponType;
        _nowMonsterWeapon.weaponDamage = weaponDamage;

        if (isRandomSetMonsterWeaponIndex)
        {
            if (listRandomSetWeaponIndexes == null || listRandomSetWeaponIndexes.Count == 0)
            {
                _nowMonsterWeapon.isRandomSetWeaponIndex = isRandomSetMonsterWeaponIndex;
                _nowMonsterWeapon.nowWeaponIndex = monsterWeaponIndex;
            }
            else
            {
                _nowMonsterWeapon.isRandomSetWeaponIndex = false;
                _nowMonsterWeapon.nowWeaponIndex =
                    listRandomSetWeaponIndexes[Random.Range(0, listRandomSetWeaponIndexes.Count)];
            }
        }
        else
        {
            _nowMonsterWeapon.isRandomSetWeaponIndex = isRandomSetMonsterWeaponIndex;
            _nowMonsterWeapon.nowWeaponIndex = monsterWeaponIndex;
        }
        

        _nowMonsterStateMachine.monsterWeapon = _nowMonsterWeapon;
        _nowMonsterWeapon.monsterStateMachine = _nowMonsterStateMachine;

        _nowMonsterHpObject = ObjectPoolManager.Instance.GetObject(E_ObjectType.MonsterHp);
        _nowMonsterHpController = _nowMonsterHpObject.GetComponent<MonsterHpController>();
        _nowMonsterHpController.monsterCharacter = _nowMonsterStateMachine.monsterCharacter;
        _nowMonsterStateMachine.monsterCharacter.maxHp = monsterHp;
        _nowMonsterStateMachine.monsterCharacter.monsterHpController = _nowMonsterHpController;
        _nowMonsterHpObject.SetActive(false);
        
        _nowMonsterObject.SetActive(true);
        _nowMonsterWeaponObject.SetActive(true);
        
        yield return new WaitUntil(()=> _nowMonsterStateMachine.isAwakeInit && _nowMonsterStateMachine.monsterCharacter.isAwakeInit &&
                                        _nowMonsterWeapon.isAwakeInit);
        _nowMonsterStateMachine.Init();
        _nowMonsterStateMachine.monsterCharacter.Init();
        _nowMonsterHpController.Init();
        _nowMonsterWeapon.Init();
    }
}
