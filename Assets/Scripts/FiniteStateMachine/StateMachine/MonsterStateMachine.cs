using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MonsterStateMachine : StateMachine
{
    public Animator animator;
    public Transform playerTransform;
    public Collider monsterCollider;
    public Rigidbody monsterRigidbody;
    public MonsterCreator monsterCreator;

    public MonsterWeapon monsterWeapon;
    public Transform weaponEquipFatherTransform;
    public MonsterCharacter monsterCharacter;
    
    public MonsterIdle IdleState;
    public MonsterHit HitState;
    public MonsterDeath DeathState;
    public MonsterSeePlayer SeePlayerState;
    public MonsterDefense DefenseState;
    public MonsterWalk WalkState;
    public MonsterRun RunState;
    public MonsterTurn TurnState;
    public MonsterAttack AttackState;
    public MonsterDodge DodgeState;

    [Header("怪物AI部分")]
    // 怪物AI
    public List<Transform> listPatrolPoints;
    // 当前怪物是否具有巡逻点
    public bool isHasPatrolPoints;
    // 是否已经发现玩家
    public bool isSeePlayer;
    // 发现玩家最大距离
    public float seePlayerDistance;
    // 开始攻击玩家距离
    public float fightWithPlayerDistance;
    // 开始脱离追踪玩家状态距离
    public float stopFollowPlayerDistance;
    // 可视角度(与怪物正前方向量在水平面的角度)
    [Range(0,90f)]
    public float viewAngle;
    // 战斗时攻击倾向权重
    public float attackWeight;
    // 战斗时防御倾向权重
    public float defenseWeight;
    // 战斗时闪避倾向权重
    public float avoidWeight;
    // 总权重
    private float _allWeight;
    // 最大静止时长
    public float maxIdleTime;
    // 怪物最大旋转时间90度内
    public float rotateMaxTime;
    [Header("怪物动画部分")] 
    // 怪物正常攻击动画数量
    public int monsterNormalAttackCount;
    // 怪物连击攻击动画数量
    public int monsterComboAttackCount;
    // 怪物处决攻击动画数量
    public int monsterStrongAttackCount;
    
    public BaseState CurrentState => _currentState;
    // 当前怪物移动目标
    public Transform nowTarget;
    // 当前位于某一巡逻点或前往某一巡逻点的巡逻点的索引值
    public int nowPatrolPointIndex;
    // 当前与目标点的方向向量
    public Vector3 monsterToTargetDirection;
    // 当前与目标点的距离
    public float monsterToTargetDistance;
    // 玩家距离自己距离
    public float playerDistance;
    // 从自己到玩家在Xoz平面的方向向量
    public Vector3 monsterToPlayerDirection;
    // 当前到玩家向量与forward向量在Xoz平面的夹角
    public float nowAngle;
    // 当前怪物目标旋转四元数
    public Quaternion targetRotation;
    // 当前是否在进行旋转
    public bool isRotating;
    // 当前旋转时间
    public float nowRotateTime;
    // 是否停止旋转协程
    public bool isStopRotateCoroutine;
    // 是否锁定旋转
    public bool isFreezeRotation;
    // 是否是主摄像机选定的跟踪目标
    public bool isMainCameraFollowing;
    
    // 玩家位置满足怪物看见的情况下由怪物向玩家发射检测是否存在阻挡的射线检测
    private RaycastHit _monsterToPlayerRaycastHit;
    // Animator中参数的索引字典
    [HideInInspector] public Dictionary<string, int> DicAnimatorIndexes;
    // Animator中参数表
    private AnimatorControllerParameter[] _animatorControllerParameters;
    // 怪物90度角度内缓慢旋转协程
    private Coroutine _monsterToTargetRotationCoroutine;
    // 临时计数变量
    private int _count;
    // 怪物90度内旋转协程计时器
    private float _rotateTimer;
    // fixedUpdate协程返回值
    private WaitForFixedUpdate _waitForFixedUpdate;
    // 选择战斗行为的随机值
    private float _randomValueOnSelectFightState;
    // 是否进行过唤醒初始化
    public bool isAwakeInit;

    protected override void Awake()
    {
        AwakeInitParameters();
        base.Awake();
    }

    protected void Start()
    {
        Init();
        
        playerTransform = PlayerMovementStateMachine.Instance.playerTransform;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        if (_currentState == null || _currentState.state == E_State.Death)
        {
            base.FixedUpdate();
            return;
        }

        if (monsterCharacter.hp <= 0 && _currentState.state != E_State.Death)
        {
            ChangeState(DeathState);
        }
        
        UpdateToPlayerAndToTargetDirectionAndDistanceAndAngle();
        base.FixedUpdate();
    }

    public override bool ChangeState(BaseState newState)
    {
        if (newState == null || _currentState == null)
        {
            return false;
        }
        
        return base.ChangeState(newState);
    }

    public override BaseState GetInitialState()
    {
        return IdleState;
    }

    private void AwakeInitParameters()
    {
        isAwakeInit = true;
        
        IdleState = new MonsterIdle(this);
        HitState = new MonsterHit(this);
        DeathState = new MonsterDeath(this);
        SeePlayerState = new MonsterSeePlayer(this);
        DefenseState = new MonsterDefense(this);
        WalkState = new MonsterWalk(this);
        RunState = new MonsterRun(this);
        TurnState = new MonsterTurn(this);
        AttackState = new MonsterAttack(this);
        DodgeState = new MonsterDodge(this);
        
        _animatorControllerParameters = animator.parameters;
        DicAnimatorIndexes = new Dictionary<string, int>();
        for (_count = 0; _count < _animatorControllerParameters.Length; _count++)
        {
            DicAnimatorIndexes.Add(_animatorControllerParameters[_count].name, _animatorControllerParameters[_count].nameHash);
        }

        _allWeight = attackWeight + defenseWeight + avoidWeight;
        _waitForFixedUpdate = new WaitForFixedUpdate();
    }
    // 手动初始化
    public void Init()
    {
        // 初始化参数
        InitParameters();
        
        GameManager.Instance.AddMonsterInListMonsters(this);
        
        monsterRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        if (_currentState != null && _currentState.state != E_State.Idle)
        {
            ChangeState(IdleState);
            animator.SetTrigger(DicAnimatorIndexes["ToIdle"]);
        }

        if (transform.localScale.x > 1f)
        {
            fightWithPlayerDistance += fightWithPlayerDistance * (transform.localScale.x - 1f) * 0.6f;    
        }
        
    }

    private void InitParameters()
    {
        monsterCollider.enabled = true;
        monsterRigidbody.useGravity = true;

        if (listPatrolPoints.Count != 0)
        {
            isHasPatrolPoints = true;
        }
        else
        {
            isHasPatrolPoints = false;
        }

        // 默认不在任何巡逻点
        nowPatrolPointIndex = -1;

        isStopRotateCoroutine = false;

        isSeePlayer = false;
    }

    // 更新玩家距离，目标方向，在距离满足条件时更新与怪物到玩家方向与怪物正前方之间的角度
    private void UpdateToPlayerAndToTargetDirectionAndDistanceAndAngle()
    {
        if (playerTransform != null)
        {
            if (!(PlayerMovementStateMachine.Instance is null) &&
                PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Death)
            {
                isSeePlayer = false;
                return;
            }
            
            monsterToPlayerDirection =
                Vector3.ProjectOnPlane(playerTransform.position - transform.position, Vector3.up);
            playerDistance = monsterToPlayerDirection.magnitude;
            
            if (!isSeePlayer)
            {
                // 发现玩家
                if (playerDistance <= seePlayerDistance && CheckPlayerIsInView())
                {
                    isSeePlayer = true;
                    nowTarget = playerTransform;
                    
                    nowPatrolPointIndex = -1;
                    monsterToTargetDirection = monsterToPlayerDirection;
                    monsterToTargetDistance = playerDistance;
                    
                    ChangeState(SeePlayerState);
                }
                else
                {
                    if (nowPatrolPointIndex != -1)
                    {
                        monsterToTargetDirection =
                            Vector3.ProjectOnPlane(listPatrolPoints[nowPatrolPointIndex].position - transform.position,
                                Vector3.up);
                        monsterToTargetDistance = monsterToTargetDirection.magnitude;
                        nowAngle = Vector3.Angle(monsterToTargetDirection, transform.forward);
                    }
                }
            }
            else
            {
                if ((PlayerMovementStateMachine.Instance != null &&
                     PlayerMovementStateMachine.Instance.CurrentState.state == E_State.Death) ||
                    playerDistance >= stopFollowPlayerDistance)
                {
                    isSeePlayer = false;
                    ChangeState(IdleState);
                    return;
                }

                nowAngle = Vector3.Angle(monsterToPlayerDirection, transform.forward);
            }
        }
    }
    
    // 检查玩家是否在视野范围内
    private bool CheckPlayerIsInView()
    {
        // 玩家在怪物前方并且与怪物正前方向量夹角满足要求，并且由怪物向玩家方向发射射线没有阻挡
        if (Vector3.Dot(monsterToPlayerDirection, transform.forward) < 0f)
        {
            return false;
        }
        else
        {
            nowAngle = Vector3.Angle(monsterToPlayerDirection, transform.forward);
            if (nowAngle > viewAngle)
            {
                return false;
            }

            return !Physics.Raycast(transform.position, monsterToPlayerDirection.normalized, playerDistance,
                InfoManager.Instance.layerMonsterSightCheck);
        }
    }
    
    // 随机获取一处巡逻点
    public void RandomSetPatrolPointToTarget()
    {
        _count = Random.Range(0, listPatrolPoints.Count);
        if (_count == nowPatrolPointIndex)
        {
            nowPatrolPointIndex = (_count + 1) % listPatrolPoints.Count;
            nowTarget = listPatrolPoints[nowPatrolPointIndex];
        }
        else
        {
            nowPatrolPointIndex = _count;
            nowTarget = listPatrolPoints[nowPatrolPointIndex];
        }
    }
    
    // 怪物当前与目标点角度检查若是角度大于90度则进入Turn状态再进行移动和缓慢旋转，若是小于90则开始缓慢旋转
    // 返回true时切换为旋转状态，返回false使用协程进行缓慢旋转
    public bool CheckNowAngle()
    {
        UpdateToTargetDirectionAndAngleImmediately();
        if (nowAngle <= 45f)
        {
            _rotateTimer = 0f;
            nowRotateTime = rotateMaxTime * nowAngle / 90f;
            targetRotation = Quaternion.LookRotation(monsterToTargetDirection);
            if (!isRotating)
            {
                isStopRotateCoroutine = false;
                _monsterToTargetRotationCoroutine = StartCoroutine(RotateToTargetRotation());
            }

            return false;
        }

        if (isRotating)
        {
            isStopRotateCoroutine = true;
        }
        return true;
    }
    
    // 旋转怪物至目标四元数
    private IEnumerator RotateToTargetRotation()
    {
        isRotating = true;
        while (_rotateTimer < nowRotateTime && !isStopRotateCoroutine)
        {
            _rotateTimer += Time.fixedDeltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotateTimer);
            yield return _waitForFixedUpdate;
        }

        isRotating = false;
    }
    
    // 立刻更新一次距离目标点方向和角度
    public void UpdateToTargetDirectionAndAngleImmediately()
    {
        if (nowPatrolPointIndex != -1 && !isSeePlayer)
        {
            monsterToTargetDirection = Vector3.ProjectOnPlane(
                listPatrolPoints[nowPatrolPointIndex].position - transform.position,
                Vector3.up);
            nowAngle = Vector3.Angle(monsterToTargetDirection, transform.forward);
        }
        else
        {
            monsterToPlayerDirection = Vector3.ProjectOnPlane(
                playerTransform.position - transform.position,
                Vector3.up);
            monsterToTargetDirection = monsterToPlayerDirection;
            nowAngle = Vector3.Angle(monsterToTargetDirection, transform.forward);
        }
    }
    
    // 根据战斗权重切换攻击防御或闪避行为
    public void ChangeStateToFight()
    {
        _randomValueOnSelectFightState = Random.Range(0f, 1f);
        // 攻击
        if (_randomValueOnSelectFightState <= attackWeight)
        {
            // 选择攻击类型
            // 连招攻击
            if (_randomValueOnSelectFightState < attackWeight * 0.5f)
            {
                AttackState.AttackType = E_AttackType.ComboAttack;
            }
            // 普通攻击
            else
            {
                AttackState.AttackType = E_AttackType.NormalAttack;
            }
            ChangeState(AttackState);
        }
        else
        {
            _randomValueOnSelectFightState -= attackWeight;
            // 防御
            if (_randomValueOnSelectFightState <= defenseWeight)
            {
                ChangeState(DefenseState);
            }
            // 闪避
            else
            {
                ChangeState(DodgeState);
            }
        }
    }
    
    // 检查是否满足开始进入战斗的条件并进入随机的战斗状态
    public bool CheckPlayerDistanceAndAngleReadyToFight()
    {
        if (isSeePlayer && playerDistance <= fightWithPlayerDistance)
        {
            if (CheckNowAngle())
            {
                ChangeState(TurnState);
                return true;
            }
            else
            {
                ChangeStateToFight();
                return true;
            }
        }
        else if (isSeePlayer)
        {
            if (_currentState.state != E_State.Run)
            {
                ChangeState(RunState);
            }
        }

        return false;
    }

    public void RecyclingSelf()
    {
        GameManager.Instance.RemoveMonsterInListMonsters(this);
        if (monsterCreator != null)
        {
            monsterCreator.nowMonsterCount--;
        }
        ObjectPoolManager.Instance.RecyclingObject(E_ObjectType.Monster, transform.gameObject);
    }
    
    // 动画事件-----
    
    public void ChooseAndChangeNextState()
    {
        switch (_currentState.state)
        {
            case E_State.Hit:
                if (monsterCharacter.hp <= 0f)
                {
                    ChangeState(DeathState);
                }
                else if(CheckPlayerDistanceAndAngleReadyToFight())
                {
                    return;
                }
                else
                {
                    ChangeState(IdleState);
                }
                break;
            case E_State.Dodge:
                CheckPlayerDistanceAndAngleReadyToFight();
                break;
            case E_State.Defense:
                CheckPlayerDistanceAndAngleReadyToFight();
                break;
            case E_State.Attack:
                CheckPlayerDistanceAndAngleReadyToFight();
                break;
            case E_State.SeePlayer:
                ChangeState(RunState);
                break;
            case E_State.Turn:
                if (TurnState.preState.state != E_State.Turn)
                {
                    ChangeState(TurnState.preState);
                }
                else
                {
                    ChangeState(RunState);
                }
                break;
        }
    }

    // 进入防御
    public void StartDefense()
    {
        DefenseState.isDefensing = true;
    }

    public void EndDefense()
    {
        DefenseState.isDefensing = false;
    }
    
    // 开始攻击
    public void StartAttack()
    {
        monsterWeapon.nowCollider.enabled = true;
        monsterWeapon.isHit = false;
        monsterWeapon.isAttacking = true;
    }
    // 结束攻击
    public void EndAttack()
    {
        monsterWeapon.nowCollider.enabled = false;
        monsterWeapon.isAttacking = false;
    }
}
