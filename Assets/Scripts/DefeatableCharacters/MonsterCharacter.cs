using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterCharacter : DefeatableCharacter
{
    public MonsterStateMachine stateMachine;

    public Transform monsterModelsFatherTransform;
    public List<Transform> listMonsterModels;
    public int nowMonsterModelIndex = -1;
    public float maxHp;
    public MonsterHpController monsterHpController;
    public float hpUIShowTime;
    public float hpUIShowTimer;
    private Coroutine _hpUIShowCoroutine;
    private WaitForFixedUpdate _waitForFixedUpdate;
    public bool isShowHpUI;
    
    private int _count;
    public bool isAwakeInit;
    private void Start()
    {
        if (!isAwakeInit)
        {
            Init();
        }
    }

    public override bool Hit(float damage, Vector3 hitPosition, ICounterattack counterattack, bool isStrongAttack)
    {
        hpUIShowTimer = 0f;
        if (!isShowHpUI)
        {
            _hpUIShowCoroutine = StartCoroutine(ShowHpUI());
        }
        
        // 闪避状态
        if (stateMachine.CurrentState.state == E_State.Dodge ||
            stateMachine.CurrentState.state == E_State.Death)
        {
            return false;
        }
        
        // 防御状态则对方攻击不造成击中效果
        if (counterattack != null && stateMachine.CurrentState.state == E_State.Defense && stateMachine.DefenseState.isDefensing && !isStrongAttack)
        {
            stateMachine.DefenseState.isSuccessfulDefense = true;
            counterattack.Counterattack(0.1f,
                stateMachine.monsterCollider.ClosestPoint(hitPosition));
            return true;
        }
        
        //攻击状态弹刀
        if (counterattack != null && stateMachine.monsterWeapon.isAttacking && !isStrongAttack)
        {
            stateMachine.monsterWeapon.Counterattack(0.1f, hitPosition);
            counterattack.Counterattack(0.1f,
                stateMachine.monsterCollider.ClosestPoint(hitPosition));
            return true;
        }

        stateMachine.HitState.HitPosition = hitPosition;
        stateMachine.ChangeState(stateMachine.HitState);
        return base.Hit(damage, hitPosition, counterattack, isStrongAttack);
    }

    public override void Death()
    {
        stateMachine.ChangeState(stateMachine.DeathState);
    }

    private void AwakeInitParams()
    {
        isAwakeInit = true;
        stateMachine = transform.GetComponent<MonsterStateMachine>();
        for (_count = 0; _count < monsterModelsFatherTransform.childCount; _count++)
        {
            listMonsterModels.Add(monsterModelsFatherTransform.GetChild(_count));
        }

        _waitForFixedUpdate = new WaitForFixedUpdate();
    }

    public void Init()
    {
        if (!isAwakeInit)
        {
            AwakeInitParams();
        }
        
        InitParams();
    }

    public void InitParams()
    {
        if (nowMonsterModelIndex == -1)
        {
            RandomGetMonsterModelIndex();
        }
        
        for (_count = 0; _count < monsterModelsFatherTransform.childCount; _count++)
        {
            if (_count == nowMonsterModelIndex)
            {
                listMonsterModels[_count].gameObject.SetActive(true);
            }
            else
            {
                listMonsterModels[_count].gameObject.SetActive(false);
            }
        }

        hp = maxHp;
        isShowHpUI = false;
    }
    
    private void RandomGetMonsterModelIndex()
    {
        nowMonsterModelIndex = Random.Range(0, listMonsterModels.Count);
    }

    private IEnumerator ShowHpUI()
    {
        isShowHpUI = true;
        monsterHpController.gameObject.SetActive(true);
        while (hpUIShowTimer < hpUIShowTime)
        {
            hpUIShowTimer += Time.fixedDeltaTime;
            monsterHpController.UpdatePosition();
            yield return _waitForFixedUpdate;
        }
        monsterHpController.gameObject.SetActive(false);
        isShowHpUI = false;
    }

    public override void ChangeHp(float value)
    {
        base.ChangeHp(value);
        monsterHpController.UpdateHpUI();
    }
}
