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

    public MonsterWeapon monsterWeapon;
    public Transform weaponEquipFatherTransform;
    
    public MonsterIdle IdleState;
    public MonsterHit HitState;
    public MonsterDeath DeathState;
    public MonsterSeePlayer SeePlayerState;
    public MonsterDefense DefenseState;
    public MonsterWalk WalkState;
    public MonsterRun RunState;
    public MonsterTurn TurnState;

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
    [Range(0f, 1f)]
    // 怪物90度角度内缓慢旋转旋转速度
    public float rotateSpeed;
    // 怪物最大旋转时间90度内
    public float rotateMaxTime;
    
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
    public void Awake()
    {
        AwakeInitParameters();
        Init();
    }

    protected override void Start()
    {
        base.Start();
        
        playerTransform = PlayerMovementStateMachine.Instance.playerTransform;
    }

    protected override void Update()
    {
        
        base.Update();
    }

    protected override void FixedUpdate()
    {
        UpdateToPlayerAndToTargetDirectionAndDistanceAndAngle();
        base.FixedUpdate();
    }

    public override bool ChangeState(BaseState newState)
    {
        if (_currentState.state == E_State.Death)
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
        IdleState = new MonsterIdle(this);
        HitState = new MonsterHit(this);
        DeathState = new MonsterDeath(this);
        SeePlayerState = new MonsterSeePlayer(this);
        DefenseState = new MonsterDefense(this);
        WalkState = new MonsterWalk(this);
        RunState = new MonsterRun(this);
        TurnState = new MonsterTurn(this);

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
    }

    private void InitParameters()
    {
        monsterCollider.enabled = true;
        monsterRigidbody.useGravity = true;
        
        if (monsterWeapon != null)
        {
            monsterWeapon.monsterStateMachine = this;
        }

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
    }

    // 更新玩家距离，目标方向，在距离满足条件时更新与怪物到玩家方向与怪物正前方之间的角度
    private void UpdateToPlayerAndToTargetDirectionAndDistanceAndAngle()
    {
        if (playerTransform != null)
        {
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

            return Physics.Raycast(transform.position, monsterToPlayerDirection.normalized, playerDistance,
                InfoManager.Instance.layerMonsterSightCheck);
        }
    }
    
    // 随机获取一处巡逻点
    public Transform RandomGetPatrolPoint()
    {
        _count = Random.Range(0, listPatrolPoints.Count);
        if (_count == nowPatrolPointIndex)
        {
            return listPatrolPoints[(_count + 1) % listPatrolPoints.Count];
        }
        else
        {
            return listPatrolPoints[_count];
        }
    }
    
    // 怪物当前与目标点角度检查若是角度大于90度则进入Turn状态再进行移动和缓慢旋转，若是小于90则开始缓慢旋转
    // 返回true时切换为旋转状态，返回false使用协程进行缓慢旋转
    public bool CheckNowAngle()
    {
        if (nowAngle < 90f)
        {
            _rotateTimer = 0f;
            nowRotateTime = rotateMaxTime * nowAngle / 90f;
            Debug.Log("nowAngle = " + nowAngle + "\n"
                + "monsterToTargetDirection = " + monsterToTargetDirection);
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
    
    // 动画事件-----
    
    public void ChooseAndChangeNextState()
    {
        switch (_currentState.state)
        {
            case E_State.Idle:
                break;
            case E_State.Walk:
                break;
            case E_State.Run:
                break;
            case E_State.Jump:
                break;
            case E_State.Fall:
                break;
            case E_State.Sliding:
                break;
            case E_State.WallRunning:
                break;
            case E_State.Climb:
                break;
            case E_State.Roll:
                break;
            case E_State.Grapple:
                break;
            case E_State.Fight:
                break;
            case E_State.Hit:
                break;
            case E_State.Death:
                break;
            case E_State.Dodge:
                break;
            case E_State.Defense:
                break;
            case E_State.Attack:
                break;
            case E_State.StrongAttack:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
