using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum E_State
{
    Idle = 0,
    Walk = 1,
    Run = 2,
    Squat = 3,
    Jump = 4,
    Fall = 5,
    Sliding = 6,
    WallRunning = 7,
    Climb = 8,
    Roll = 9,
    Grapple = 10,
    Fight = 11,
    Hit = 12,
    Death = 13,
    // 以下为Monster独有
    Dodge = 14,
    Defense = 15,
    Attack = 16,
    SeePlayer = 17,
    Turn = 18,
}

public struct MoveInputInformation
{
    public bool MoveForwardInput;
    public bool MoveBackwardInput;
    public bool MoveLeftInput;
    public bool MoveRightInput;
    public int VerticalInput;
    public int HorizontalInput;
    public bool JumpInput;
    public bool RunInput;
    public bool SquatInput;
    public bool SlidingInput;
    public bool RollInput;
}

public class PlayerMovementStateMachine : StateMachine
{
    private static PlayerMovementStateMachine _instance;
    public static PlayerMovementStateMachine Instance => _instance;
    
    #region Component

    // 组件
    // 动画状态机
    public Animator playerAnimator;
    public Transform headRaycastEmissionTransform;
    public Transform footRaycastEmissionTransform;
    public Transform playerTransform;
    public Rigidbody playerRigidbody;
    // 碰撞盒父节点
    public Transform colliders;
    // 正常状态下的碰撞盒
    public CapsuleCollider baseCollider;
    // 下蹲状态下的碰撞盒
    public CapsuleCollider squatCollider;
    // 滑铲状态下的碰撞盒
    public CapsuleCollider slidingCollider;
    // 人物模型状态机去同步的位置点
    public Transform playerModelRootSyncPointTransform;
    // 交互碰撞盒
    public Transform interactBox;
    // 人物角色
    public PlayerCharacter playerCharacter;
    #endregion

    #region StateObjects

    // 当前状态
    [HideInInspector] public BaseState CurrentState => _currentState;
    
    // 空闲状态
    [HideInInspector] public Idle IdleState;
    // 走路状态
    [HideInInspector] public Walk WalkState;
    // 跳跃状态
    [HideInInspector] public Jump JumpState;
    // 下落状态
    [HideInInspector] public Fall FallState;
    // 奔跑状态
    [HideInInspector] public Run RunState;
    // 下蹲状态
    [HideInInspector] public Squat SquatState;
    // 滑铲状态
    [HideInInspector] public Sliding SlidingState;
    // 滑墙状态
    [HideInInspector] public WallRunning WallRunningState;
    // 攀爬状态
    [HideInInspector] public Climb ClimbState;
    // 翻滚状态
    [HideInInspector] public Roll RollState;
    // 钩锁状态
    [HideInInspector] public Grapple GrappleState;
    // 战斗状态
    [HideInInspector] public Fight FightState;
    // 受击状态
    [HideInInspector] public Hit HitState;
    // 死亡状态
    [HideInInspector] public Death DeathState;

    #endregion
    
    #region Event

    public event Func<bool> WhenOnCollisionEnter;
    public event UnityAction WhenUpdateLast;

    public event Func<E_State, bool> ChangeStateExternalJudgmentEvent;
    
    #endregion

    #region PublicGettableParamaters

    [Header("输入参数")] [Range(0.01f, 1f)]
    // 每帧垂直和水平方向输入影响数值大小
    public float inputValueRate = 0.01f;
    // 当同时摁住互斥的摁键和同时松开互斥的摁键时是否需要以更快的速度使值归零
    public bool isQuickZeroing;
    // 快速归零时速率倍数
    public float quickZeroingRate = 2f;
    
    // 移动输入信息
    [HideInInspector] public MoveInputInformation MoveInputInfo;
    // 当前水平移动移动速度的最大值
    [HideInInspector] public float nowMoveSpeed;
    // 当前玩家是否在地面上
    [HideInInspector] public bool isOnGround;
    // 当前玩家刚体在XOZ平面上移动速度大小
    [HideInInspector] public float playerXozSpeed;
    // 当前玩家刚体在XOY平面上移动速度大小
    [HideInInspector] public float playerXoySpeed;
    // 当前玩家是否在可运动角度范围内的斜面上
    [HideInInspector] public bool isOnSlope;
    // 玩家移动的方向
    [HideInInspector] public Vector3 direction;
    // 左边是否有墙壁
    [HideInInspector] public bool hasWallOnLeft;
    // 右边是否有墙壁
    [HideInInspector] public bool hasWallOnRight;
    // 前面是否有墙壁
    [HideInInspector] public bool hasWallOnForward;
    // 人物脑袋前方是否有墙壁
    [HideInInspector] public bool hasWallOnHeadForward;
    // 人物脚前方是否有墙壁
    [HideInInspector] public bool hasWallOnFootForward;
    // 当前斜面角度
    [HideInInspector] public float slopeAngle;
    // 摄像机在XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度
    [HideInInspector] public float cameraForwardWithWallAbnormalAngle;
    // 在下落、跳跃后能否快速进入奔跑状态
    [HideInInspector] public bool isFastToRun;
    // 当前玩家距离地面高度
    [HideInInspector] public float nowHigh;
    // 是否向下使用球形检测
    [HideInInspector] public bool isUseSphereCast;
    // PlayerMovementStateMachine中参数在Animator中参数的索引字典
    [HideInInspector] public Dictionary<string, int> DicAnimatorIndexes;
    // 是否尝试改变碰撞盒到当前状态的碰撞盒(在当前位置无法正常设置碰撞盒为当前状态碰撞盒时该值为true)
    public bool IsTryToChangeState => _isTryToChangeState;
    // 尝试去切换的状态
    public BaseState TryToState => _tryToState;
    // 当前是否是移动状态动画
    public bool isMove;
    
    #endregion

    #region PublicSettablePlayerAndSceneParamaters

    [Header("玩家及场景参数")] // 以后考虑是不是要换到别的脚本里面去
    // 玩家高度
    public float playerHeight = 2f;
    // 允许玩家运动的最大斜面角度
    public float maxSlopeAngle = 45f;

    #endregion

    #region PublicSettableMovementStateParamaters

    [Header("玩家运动相关参数")]
    #region 走路
    [Header("走路")]
    // 移动
    // 走路向前的速度的最大值
    public float walkForwardSpeed = 8f;
    // 走路向后的速度的最大值
    public float walkBackwardSpeed = 5f;
    // 走路水平方向移动的速度的最大值
    public float walkHorizontalSpeed = 5f;
    // 走路时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = walkMoveForce * nowMoveSpeed)
    public float walkMoveForce = 10f;
    #endregion

    #region 跳跃
    [Header("跳跃")]
    // 跳跃
    // 是否通过给力让玩家跳跃
    // public bool jumpByForce;
    // 跳跃瞬间给玩家的力的大小
    // public float jumpForce = 10f;
    // 跳跃瞬间给玩家的速度的大小
    // public float jumpVelocity = 10f;
    // 玩家是否在地面的高度检测时的偏移量
    public float heightOffset = 0.2f;
    // 跳跃持续时间
    // public float jumpTime = 0.5f;
    // 跳跃高度
    public float jumpHigh;
    // 向下进行球形射线检测时球形的半径
    public float downSphereCastRadius = 0.25f;
    #endregion

    #region 奔跑
    [Header("奔跑")]
    // 奔跑
    // 跑步向前的速度的最大值
    public float runForwardSpeed = 20f;
    // 跑步向后的速度的最大值
    public float runBackwardSpeed = 2f;
    // 水平方向跑步速度
    public float runHorizontalSpeed = 2f;
    // 玩家从WalkToRun所需摁键时间
    public float toRunTime = 0.5f;
    // 奔跑时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = runMoveForce * nowMoveSpeed)
    public float runMoveForce = 20f;
    #endregion

    #region 下蹲
    [Header("下蹲")]
    // 下蹲
    // 下蹲时移动的速度的最大值
    public float squatSpeed = 5f;
    // 下蹲时给予玩家力的大小
    public float squatMoveForce = 8f;
    #endregion

    #region 滑铲
    [Header("滑铲")]
    // 滑铲
    // 平地上滑铲速度最大速度
    public float slidingSpeed = 12f;
    // 斜面上滑铲相对最大速度(当斜面越陡时速度越大)
    public float slidingOnSlopeSpeed = 12f;
    // 滑铲给玩家力的大小
    public float slidingMoveForce = 30f;
    // 滑铲加速时间
    public float slidingAccelerateTime = 0.5f;
    // 滑铲冷却时间(进入Run状态后需要过一段时间后才能进行滑铲)
    public float slidingCoolTime = 0.2f;
    #endregion

    #region 下落
    [Header("下落")]
    // 下落
    // 快速下落时所受重力倍数
    public float fallGravityScale = 3f;
    // 下落时水平移动的最大速度
    public float fallSpeed = 1f;
    #endregion
    
    #region 滑墙
    [Header("滑墙")]
    // 滑墙
    // 可以进行滑墙的最低高度
    public float wallRunningMinHigh = 0.5f;
    // 在墙壁上向前滑行的最大速度
    public float wallRunningForwardSpeed = 8f;
    // 在墙上滑行时驱动玩家移动力的大小
    public float wallRunningForce = 6f;
    // 在墙上滑行的最大时间
    public float wallRunningTime = 3f;
    // 检查左右前是否有可以进行滑墙的墙的距离
    public float wallCheckDistance = 1f;
    #endregion

    #region 攀爬
    [Header("攀爬")]
    // 攀爬
    // 攀爬向上移动的最大速度
    public float climbUpSpeed = 6f;
    // 攀爬左右移动的最大速度
    public float climbHorizontalSpeed = 3f;
    // 攀爬时力的大小
    public float climbForce = 5f;
    // 攀爬的最大时间
    public float climbTime = 3f;
    // 最大攀爬角度
    public float climbMaxAngle = 30f;
    // 向前进行球形射线检测时球形的半径
    public float forwardSphereCastRadius = 0.25f;
    #endregion

    #region 翻滚
    [Header("翻滚")]
    // 翻滚
    // 翻滚的最大速度
    public float rollSpeed = 5f;
    // 翻滚时力的大小
    public float rollForce = 5f;
    #endregion

    #region 钩锁
    [Header("钩锁")]
    // 钩锁
    // 钩锁移动过程中最高点距离起始点的相对高度
    public float grapplingHighestPointRelativeHigh = 2f;
    // 钩锁移动距离系数(系数为一时移动后到达位置差不多为钩锁勾中位置)
    public float grapplingHookRopeMoveVelocityRatio = 1f;
    // 钩锁移动时移动控制力的大小
    public float grapplingForce = 10f;
    // 钩锁最大速度大小
    public float grappleSpeed = 20f;

    #endregion

    #endregion

    #region PrivateInternalParamaters

    // 计算Xoz或Xoy平面玩家刚体速度用的临时变量
    private Vector3 _calVelocity;
    // 由玩家向下发射的用于检测地面与斜面用的射线的击中信息
    private RaycastHit _downRaycastHit;
    // 由玩家向左发射的用于检测玩家左侧是否有墙壁的射线的击中信息
    private RaycastHit _leftRaycastHit;
    // 由玩家向右发射的用于检测玩家左侧是否有墙壁的射线的击中信息
    private RaycastHit _rightRaycastHit;
    // 由玩家向右发射的用于检测玩家前方是否有墙壁的射线的击中信息
    private RaycastHit _forwardRaycastHit;
    // 由玩家向下发射的用于检测玩家距离地面高度的距离为玩家最高高度的射线的击中信息
    private RaycastHit _downMaxRaycastHit;
    // 由玩家头部向前发射的用于检测玩家头部前方是否有墙壁的射线的击中信息
    private RaycastHit _forwardHeadRaycastHit;
    // 由玩家脚向前发射的用于检测玩家脚前方是否有墙壁的射线的集中信息
    private RaycastHit _forwardFootRaycastHit;
    // 人物Animator的参数集
    private AnimatorControllerParameter[] _animatorControllerParameters;
    // 限制玩家速度时用来计算x方向速度大小的临时变量
    private float _calVelocityX;
    // 限制玩家速度时用来计算y方向速度大小的临时变量
    private float _calVelocityY;
    // 限制玩家速度时用来计算z方向速度大小的临时变量
    private float _calVelocityZ;
    // 限制玩家速度时用来计算当前某平面速度与最大速度的比值
    private float _velocityWithSpeedRatio;
    // 为Animator组件提供的垂直和水平方向的值(在-1到1之间有细小连续变化的Float值)
    private float _verticalInputForAnimator;
    private float _horizontalInputForAnimator;
    // 上一次从角色脚部向前发射的射线能够集中墙壁的射线的击中信息
    private RaycastHit _lastForwardFootRaycastHit;
    // 向上发射射线用于检测玩家上方距离高度的射线的击中信息
    private RaycastHit _upHighRaycastHit;
    // 向上发射射线用于检测玩家上方距离内是否存在地面层的对象
    private bool _hasGroundInDistance;
    // 向上发射射线用于检测玩家上方距离内是否存在墙层的对象
    private bool _hasWallInDistance;
    // 是否尝试改变状态
    private bool _isTryToChangeState;
    // 尝试去切换的状态
    private BaseState _tryToState;
    // SlidingCollider中心点与其gameObject对象的Transform组件中心点的偏移量
    private Vector3 _slidingCenterOffset;
    // SquatCollider中心点与其gameObject对象的Transform组件中心点的偏移量
    private Vector3 _squatCenterOffset;
    // BaseCollider中心点与其gameObject对象的Transform组件中心点的偏移量
    private Vector3 _baseCenterOffset;
    // 临时计数器
    private int _count;
    // 用来存储事件列表中所有委托的数组
    private Delegate[] _allDelegates;
    // 人物模型位置同步状态机位置时用来计算的坐标
    private Vector3 _calPlayerModelPosition;
    // 战斗状态并可旋转时旋转人物root同步至playerTransform的Rotation的协程
    private Coroutine _rotateRootToRootTargetCoroutine;
    // Root旋转的目标Rotation
    private Quaternion _rootTargetRotation;
    // 是否正在旋转至目标Rotation
    private bool _isRotateRootToTarget;
    // 旋转人物Root协程yield return变量
    private WaitForFixedUpdate _waitForFixedUpdate;
    // 是否是awake进行初始化
    private bool _isAwakeInit;
    #endregion

    protected override void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        AwakeInitParameters();
        
        _isAwakeInit = true;
        
        base.Awake();
    }

    private void Start()
    {
        // 初始化
        Init();
    }

    // 重写父类Update函数统一进行一些数据更新
    protected override void Update()
    {
        // 更新移动输入信息
        if (!(InputManager.Instance is null))
        {
            UpdateMoveInputInformation();
        }

        // 更新检查是否在地面
        UpdateIsOnGroundAndIsOnSlopeAndNowHigh();

        // 距离地面高度高于滑墙的最小高度时检测左右是否存在墙壁
        if (nowHigh >= wallRunningMinHigh)
        {
            UpdateHasWallOnLeftWithRight();
        }
        else
        {
            hasWallOnLeft = false;
            hasWallOnRight = false;
        }

        UpdateHasWallOnForward();

        UpdateAnimatorParameters();

        UpdateXozVelocity();

        DrawLine();

        // 当前状态需要尝试去进行切换
        if (_isTryToChangeState)
        {
            ChangeState(_tryToState);
        }
        
        WhenUpdateLast?.Invoke();

        base.Update();
    }

    // 重写父类FixUpdate函数在玩家处于斜面上时给予玩家手动重力
    protected override void FixedUpdate()
    {
        // 将人物模型分离出来当运动时人物坐标旋转由状态机控制，攻击时由动画状态机控制状态机位置同步人物模型位置
        if (isMove)
        {
            _calPlayerModelPosition = playerTransform.position + _baseCenterOffset;
            _calPlayerModelPosition.y -= baseCollider.height * 0.5f;
            playerAnimator.transform.position = _calPlayerModelPosition;
            playerAnimator.transform.rotation = transform.rotation;
        }
        else
        {
            if (!InfoManager.Instance.isLockAttackDirection)
            {
                _rootTargetRotation = transform.rotation;
                if (!_isRotateRootToTarget)
                {
                    _rotateRootToRootTargetCoroutine = StartCoroutine(RootRotateToRootTargetRotation());
                }
            }
            
            _calPlayerModelPosition = playerAnimator.transform.position;
            _calPlayerModelPosition.y = transform.position.y + _baseCenterOffset.y - baseCollider.height * 0.5f;
                // baseCollider.transform.position.y + _baseCenterOffset.y - baseCollider.height * 0.5f;
            playerAnimator.transform.position = _calPlayerModelPosition;
            
            _calPlayerModelPosition = playerTransform.position;
            _calPlayerModelPosition.x = playerModelRootSyncPointTransform.position.x;
            _calPlayerModelPosition.z = playerModelRootSyncPointTransform.position.z;
            // _calPlayerModelPosition.y += baseCollider.height * 0.5f + +_baseCenterOffset.y;
            playerTransform.position = _calPlayerModelPosition;
        }
        
        // 除了滑墙、攀爬和死亡重力全部在这里管理
        if (isOnSlope)
        {
            // 关闭玩家刚体自带的重力
            playerRigidbody.useGravity = false;
            switch (_currentState.state)
            {
                case E_State.Walk:
                case E_State.Run:
                case E_State.Squat:
                case E_State.Sliding:
                    playerRigidbody.AddForce(Mathf.Cos(slopeAngle) * playerRigidbody.mass * 9.18f * Vector3
                        .ProjectOnPlane(Vector3.down, _downRaycastHit.normal)
                        .normalized);
                    break;
            }
        }
        else if (_currentState != null && _currentState.state != E_State.WallRunning &&
                 _currentState.state != E_State.Climb && _currentState.state != E_State.Death)
        {
            playerRigidbody.useGravity = true;
        }

        base.FixedUpdate();
    }

    public void OnCollisionEnter(Collision other)
    {
        if (!(WhenOnCollisionEnter is null))
        {
            // 当返回值为true时代表不需要再去执行该委托，在事件中移除该委托
            _allDelegates = WhenOnCollisionEnter.GetInvocationList();
            for (_count = 0; _count < _allDelegates.Length; _count++)
            {
                if((bool)_allDelegates[_count].DynamicInvoke())
                {
                    WhenOnCollisionEnter -= (Func<bool>)_allDelegates[_count];
                }
            }
            _allDelegates = null;
            // Debug.Log("CheckIsMovedToHookCheckPoint");
        }
    }

    public override bool ChangeState(BaseState newState)
    {
        if (ChangeStateExternalJudgmentEvent != null && !ChangeStateExternalJudgmentEvent(newState.state))
        {
            return false;
        }
        
        if (_currentState.preState != null && !IsHighMatchCondition(_currentState.state, newState.state))
        {
            if (newState.state != E_State.Fight)
            {
                _isTryToChangeState = true;
                _tryToState = newState;
            }
            
            return false;
        }
        else
        {
            _isTryToChangeState = false;
            _tryToState = null;

            playerAnimator.SetInteger(DicAnimatorIndexes["PreState"], (int)_currentState.state);

            base.ChangeState(newState);
            SetColliderByCurrentState();

            // 差值移动摄像机的偏移位置达到更好的视觉效果
            if (CameraController.Instance is not null)
            {
                CameraController.Instance.SetThirdPersonFocusWithPlayerOffsetToTarget(newState.state);
            }
            
            playerAnimator.SetInteger(DicAnimatorIndexes["ToState"], (int)newState.state);
            playerAnimator.SetBool(DicAnimatorIndexes["isFastToRun"], isFastToRun);

            switch (newState.state)
            {
                case E_State.Fight:
                    playerAnimator.applyRootMotion = true;
                    isMove = false;
                    break;
                default:
                    isMove = true;
                    playerAnimator.applyRootMotion = false;
                    break;
            }
            
            return true;
        }
    }

    private void DrawLine()
    {
        // 下
        Debug.DrawLine(playerTransform.position,
            playerTransform.position + Vector3.down * (playerHeight * 0.5f + heightOffset), Color.red);

        // 左
        Debug.DrawLine(playerTransform.position,
            playerTransform.position - playerTransform.right * wallCheckDistance, Color.yellow);

        // 右
        Debug.DrawLine(playerTransform.position,
            playerTransform.position + playerTransform.right * wallCheckDistance, Color.yellow);

        // 前
        Debug.DrawLine(playerTransform.position,
            playerTransform.position + playerTransform.forward *
            (wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f)), Color.yellow);

        // 头顶前方
        Debug.DrawLine(headRaycastEmissionTransform.position,
            headRaycastEmissionTransform.position + headRaycastEmissionTransform.forward * (wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f)),
            Color.green);

        // 脚前方
        Debug.DrawLine(footRaycastEmissionTransform.position,
            footRaycastEmissionTransform.position + footRaycastEmissionTransform.forward * (wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f)),
            Color.green);

        // 方向
        Debug.DrawLine(playerTransform.position, playerTransform.position + direction * 5, Color.white);
    }

    public override BaseState GetInitialState()
    {
        return IdleState;
    }

    // 手动初始化
    public void Init()
    {
        // 初始化参数
        InitParameters();
        
        // 设置碰撞盒
        InitCollider();
    }
    
    // Awake初始化参数
    private void AwakeInitParameters()
    {
        // 初始化状态
        IdleState = new Idle(this);
        WalkState = new Walk(this);
        JumpState = new Jump(this);
        FallState = new Fall(this);
        RunState = new Run(this);
        SquatState = new Squat(this);
        SlidingState = new Sliding(this);
        WallRunningState = new WallRunning(this);
        ClimbState = new Climb(this);
        RollState = new Roll(this);
        GrappleState = new Grapple(this);
        FightState = new Fight(this);
        HitState = new Hit(this);
        DeathState = new Death(this);
        
        _animatorControllerParameters = playerAnimator.parameters;
        // 初始化Animator参数名与Hash对照表
        DicAnimatorIndexes = new Dictionary<string, int>();
        for (_count = 0; _count < _animatorControllerParameters.Length; _count++)
        {
            DicAnimatorIndexes.Add(_animatorControllerParameters[_count].name, _animatorControllerParameters[_count].nameHash);
        }
        
        // 计算碰撞盒中心点坐标的偏移量
        _slidingCenterOffset = new Vector3(slidingCollider.center.x * slidingCollider.transform.localScale.x,
            slidingCollider.center.y * slidingCollider.transform.localScale.y,
            slidingCollider.center.z * slidingCollider.transform.localScale.z);
        _squatCenterOffset = new Vector3(squatCollider.center.x * squatCollider.transform.localScale.x,
            squatCollider.center.y * squatCollider.transform.localScale.y,
            squatCollider.center.z * squatCollider.transform.localScale.z);
        _baseCenterOffset = new Vector3(baseCollider.center.x * baseCollider.transform.localScale.x,
            baseCollider.center.y * baseCollider.transform.localScale.y,
            baseCollider.center.z * baseCollider.transform.localScale.z);
        
        _waitForFixedUpdate = new WaitForFixedUpdate();
    }
    
    // 初始化参数
    private void InitParameters()
    {
        nowMoveSpeed = 0f;
        
        _isTryToChangeState = false;

        playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        if(!_isAwakeInit)
        {
            ChangeState(GetInitialState());
        }
        else
        {
            _isAwakeInit = false;
        }
        
        CameraController.Instance.UpdateCameraPositionAndRotationImmediately();
    }


    // 更新移动的输入信息(前到后1到-1，左到右-1到1);
    private void UpdateMoveInputInformation()
    {
        MoveInputInfo.MoveForwardInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]);
        MoveInputInfo.MoveBackwardInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveBackward]);
        MoveInputInfo.MoveLeftInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]);
        MoveInputInfo.MoveRightInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]);

        // 更新用于在状态机的不同状态中判断键盘输入的类型为int的Input值
        MoveInputInfo.VerticalInput =
            (MoveInputInfo.MoveForwardInput ? 1 : 0) + (MoveInputInfo.MoveBackwardInput ? -1 : 0);
        MoveInputInfo.HorizontalInput =
            (MoveInputInfo.MoveRightInput ? 1 : 0) + (MoveInputInfo.MoveLeftInput ? -1 : 0);

        MoveInputInfo.JumpInput = Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Jump]);
        MoveInputInfo.RunInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Run]);
        MoveInputInfo.SquatInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Squat]);
        MoveInputInfo.SlidingInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Sliding]);
        MoveInputInfo.RollInput = Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Roll]);
    }

    // 更新MoveInputInfo中VerticalInput与HorizontalInput的值
    private void UpdateVerticalInputWithHorizontalInputForAnimator()
    {
        // 垂直方向
        if (((MoveInputInfo.MoveForwardInput && MoveInputInfo.MoveBackwardInput) ||
             (!MoveInputInfo.MoveForwardInput && !MoveInputInfo.MoveBackwardInput)))
        {
            if (_verticalInputForAnimator > 0f)
            {
                _verticalInputForAnimator -= inputValueRate * (isQuickZeroing ? quickZeroingRate : 1f);
                if (_verticalInputForAnimator < 0f)
                {
                    _verticalInputForAnimator = 0f;
                }
            }
            else if (_verticalInputForAnimator < 0f)
            {
                _verticalInputForAnimator += inputValueRate * (isQuickZeroing ? quickZeroingRate : 1f);
                if (_verticalInputForAnimator > 0f)
                {
                    _verticalInputForAnimator = 0f;
                }
            }
        }
        else
        {
            // 当水平方向有摁键输入且水平方向摁键的值不同，判断是否要快速改变摁键值至0
            if (MoveInputInfo.MoveForwardInput)
            {
                if (_verticalInputForAnimator < 0f && isQuickZeroing)
                {
                    _verticalInputForAnimator += inputValueRate * quickZeroingRate;
                }
                else
                {
                    _verticalInputForAnimator += inputValueRate;
                }
            }
            else
            {
                if (_verticalInputForAnimator > 0f && isQuickZeroing)
                {
                    _verticalInputForAnimator -= inputValueRate * quickZeroingRate;
                }
                else
                {
                    _verticalInputForAnimator -= inputValueRate;
                }
            }

            if (_verticalInputForAnimator > 1f)
            {
                _verticalInputForAnimator = 1f;
            }

            if (_verticalInputForAnimator < -1f)
            {
                _verticalInputForAnimator = -1f;
            }
        }

        // 水平方向
        if (((MoveInputInfo.MoveLeftInput && MoveInputInfo.MoveRightInput) ||
             (!MoveInputInfo.MoveLeftInput && !MoveInputInfo.MoveRightInput)))
        {
            if (_horizontalInputForAnimator > 0f)
            {
                _horizontalInputForAnimator -= inputValueRate * (isQuickZeroing ? quickZeroingRate : 1f);
                if (_horizontalInputForAnimator < 0f)
                {
                    _horizontalInputForAnimator = 0f;
                }
            }
            else if (_horizontalInputForAnimator < 0f)
            {
                _horizontalInputForAnimator += inputValueRate * (isQuickZeroing ? quickZeroingRate : 1f);
                if (_horizontalInputForAnimator > 0f)
                {
                    _horizontalInputForAnimator = 0f;
                }
            }
        }
        else
        {
            // 当垂直方向有摁键输入且垂直方向摁键的值不同，判断是否要快速改变摁键值至0
            if (MoveInputInfo.MoveRightInput)
            {
                if (_horizontalInputForAnimator < 0f && isQuickZeroing)
                {
                    _horizontalInputForAnimator += inputValueRate * quickZeroingRate;
                }
                else
                {
                    _horizontalInputForAnimator += inputValueRate;
                }
            }
            else
            {
                if (_horizontalInputForAnimator > 0f && isQuickZeroing)
                {
                    _horizontalInputForAnimator -= inputValueRate * quickZeroingRate;
                }
                else
                {
                    _horizontalInputForAnimator -= inputValueRate;
                }
            }

            if (_horizontalInputForAnimator > 1f)
            {
                _horizontalInputForAnimator = 1f;
            }

            if (_horizontalInputForAnimator < -1f)
            {
                _horizontalInputForAnimator = -1f;
            }
        }
    }

    // 更新当前是否处于地面与玩家当前是否在可运动角度范围内的斜面上
    private void UpdateIsOnGroundAndIsOnSlopeAndNowHigh()
    {
        if (!(InfoManager.Instance is null))
        {
            // 向下使用球形射线检测
            if (isUseSphereCast)
            {
                // 当可以用射线检测检测到地面时放弃使用球形检测
                isOnGround = Physics.Raycast(playerTransform.position, Vector3.down,
                    out _downRaycastHit,
                    playerHeight * 0.5f + heightOffset, InfoManager.Instance.layerGroundCheck);

                if (isOnGround)
                {
                    isUseSphereCast = false;
                }
                else
                {
                    // 球形检测部分
                    isOnGround = Physics.SphereCast(playerTransform.position, downSphereCastRadius, Vector3.down,
                        out _downRaycastHit,
                        playerHeight * 0.5f + heightOffset - downSphereCastRadius, InfoManager.Instance.layerGroundCheck);
                }
            }
            else
            {
                isOnGround = Physics.Raycast(playerTransform.position, Vector3.down,
                    out _downRaycastHit,
                    playerHeight * 0.5f + heightOffset, InfoManager.Instance.layerGroundCheck);
            }

            // 在地面上
            if (isOnGround)
            {
                // 更新玩家当前距离地面高度
                nowHigh = Vector3.Distance(playerTransform.position, _downRaycastHit.point);
                // 更新玩家刚体阻力
                playerRigidbody.drag = InfoManager.Instance.groundDrag + InfoManager.Instance.airDrag;
                // 更新玩家当前是否在可运动角度范围内的斜面上

                // 排除玩家在攀爬的过程中可能会得到是斜面的可能
                if (_downRaycastHit.transform.gameObject.layer != InfoManager.Instance.layerWall)
                {
                    // 计算当前斜面的角度,_downRaycastHit.normal是击中点的面法线向量,自己想一下,通过角度转换就会得到这个角度就是斜面的斜率
                    slopeAngle = Vector3.Angle(Vector3.up, _downRaycastHit.normal);
                    if (slopeAngle != 0 && slopeAngle < maxSlopeAngle)
                    {
                        isOnSlope = true;
                    }
                    else
                    {
                        // 在大于最大允许移动角度的斜面上或平面上
                        isOnSlope = false;

                        // 在大于最大允许移动角度的斜面上,驱使玩家离开该斜面
                        if (slopeAngle >= maxSlopeAngle)
                        {
                            isOnGround = false;
                            // 给予玩家一个力将玩家弹离斜面(类似与APEX中那种不可以踩的斜面)
                            playerRigidbody.AddForce(_downRaycastHit.normal * 10f);
                        }
                    }
                }
            }
            // 没在地面上(向下的射线未射到地面)
            else
            {
                nowHigh = Physics.Raycast(playerTransform.position, Vector3.down, out _downMaxRaycastHit,
                    InfoManager.Instance.maxHigh, InfoManager.Instance.layerGroundCheck)
                    ? Vector3.Distance(_downMaxRaycastHit.point, playerTransform.position)
                    : InfoManager.Instance.maxHigh;
                isOnSlope = false;
                playerRigidbody.drag = InfoManager.Instance.airDrag;
            }
            
            // 如果是跳跃下落钩锁状态取消地面阻力
            if (_currentState.state == E_State.Jump ||
                _currentState.state == E_State.Fall ||
                _currentState.state == E_State.Grapple)
            {
                playerRigidbody.drag = InfoManager.Instance.airDrag;
            }
        }
    }

    // 向左右发射射线检测并且更新当前左边右边和前面的最大检测距离内是否存在可供滑行的墙壁
    private void UpdateHasWallOnLeftWithRight()
    {
        // 向左发射射线检测是否存在墙壁
        hasWallOnLeft = Physics.Raycast(playerTransform.position, -playerTransform.right, out _leftRaycastHit,
            wallCheckDistance,
            InfoManager.Instance.layerWallCheck);

        // 向右发射射线检测是否存在墙壁
        hasWallOnRight = Physics.Raycast(playerTransform.position, playerTransform.right, out _rightRaycastHit,
            wallCheckDistance,
            InfoManager.Instance.layerWallCheck);
    }

    // 向前方发射射线检测是否存在墙壁
    private void UpdateHasWallOnForward()
    {
        // 向前发射射线检测是否存在墙壁
        hasWallOnForward = Physics.SphereCast(playerTransform.position, forwardSphereCastRadius,
            playerTransform.forward,
            out _forwardRaycastHit,
            wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f) - forwardSphereCastRadius,
            InfoManager.Instance.layerWallCheck);

        // 在头上向前发射射线检测是否存在墙壁
        hasWallOnHeadForward = Physics.Raycast(headRaycastEmissionTransform.position, headRaycastEmissionTransform.forward, out _forwardHeadRaycastHit,
            wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f), InfoManager.Instance.layerWallCheck);

        // 在脚前方向前发射射线检测是否存在墙壁
        hasWallOnFootForward = Physics.Raycast(footRaycastEmissionTransform.position, footRaycastEmissionTransform.forward, out _forwardFootRaycastHit,
            wallCheckDistance * Mathf.Tan(climbMaxAngle * Mathf.PI / 180f), InfoManager.Instance.layerWallCheck);

        // 脚前面有墙
        if (hasWallOnFootForward)
        {
            _lastForwardFootRaycastHit = _forwardFootRaycastHit;
        }

        // 当前前方存在墙壁时更新计算cameraForwardWithWallAbnormalAngle
        if (hasWallOnForward)
        {
            UpdateCameraForwardWithWallAbnormalAngle();
        }
    }

    

    // 在Animator组件中更新参数
    private void UpdateAnimatorParameters()
    {
        // 更新用于在Animator组件里面给变量HorizontalInput和VerticalInput赋值的类型为float的Input值
        UpdateVerticalInputWithHorizontalInputForAnimator();

        if (!(DicAnimatorIndexes is null))
        {
            playerAnimator.SetBool(DicAnimatorIndexes["MoveForwardInput"], MoveInputInfo.MoveForwardInput);
            playerAnimator.SetBool(DicAnimatorIndexes["MoveBackwardInput"], MoveInputInfo.MoveBackwardInput);
            playerAnimator.SetBool(DicAnimatorIndexes["MoveLeftInput"], MoveInputInfo.MoveLeftInput);
            playerAnimator.SetBool(DicAnimatorIndexes["MoveRightInput"], MoveInputInfo.MoveRightInput);
            playerAnimator.SetFloat(DicAnimatorIndexes["VerticalInputFloat"], _verticalInputForAnimator);
            playerAnimator.SetFloat(DicAnimatorIndexes["HorizontalInputFloat"], _horizontalInputForAnimator);
            playerAnimator.SetInteger(DicAnimatorIndexes["VerticalInputInt"], MoveInputInfo.VerticalInput);
            playerAnimator.SetInteger(DicAnimatorIndexes["HorizontalInputInt"], MoveInputInfo.HorizontalInput);
            playerAnimator.SetBool(DicAnimatorIndexes["JumpInput"], MoveInputInfo.JumpInput);
            playerAnimator.SetBool(DicAnimatorIndexes["RunInput"], MoveInputInfo.RunInput);
            playerAnimator.SetBool(DicAnimatorIndexes["SquatInput"], MoveInputInfo.SquatInput);
            playerAnimator.SetBool(DicAnimatorIndexes["SlidingInput"], MoveInputInfo.SlidingInput);
            playerAnimator.SetBool(DicAnimatorIndexes["isOnGround"], isOnGround);
            playerAnimator.SetBool(DicAnimatorIndexes["isOnSlope"], isOnSlope);
            playerAnimator.SetBool(DicAnimatorIndexes["hasWallOnLeft"], hasWallOnLeft);
            playerAnimator.SetBool(DicAnimatorIndexes["hasWallOnRight"], hasWallOnRight);
            playerAnimator.SetBool(DicAnimatorIndexes["hasWallOnForward"], hasWallOnForward);
            playerAnimator.SetBool(DicAnimatorIndexes["hasWallOnHeadForward"], hasWallOnHeadForward);
            playerAnimator.SetBool(DicAnimatorIndexes["hasWallOnFootForward"], hasWallOnFootForward);
        }
    }

    // 获取当前状态的状态名(状态名为状态类内部成员参数string name)
    public string GetNowStateString()
    {
        if (_currentState is null)
        {
            return "Null";
        }

        return GetStateString(_currentState.state);
    }

    // 获取对应状态枚举的字符串状态名
    public string GetStateString(E_State state)
    {
        return Enum.GetName(typeof(E_State), state);
    }

    // 限制玩家水平速度值在当前最大速度内
    public void ClampXozVelocity()
    {
        UpdateXozVelocity();
        if (playerXozSpeed > nowMoveSpeed)
        {
            _velocityWithSpeedRatio = nowMoveSpeed / playerXozSpeed;
            _calVelocity.x = playerRigidbody.velocity.x * _velocityWithSpeedRatio;

            // 斜面的话y轴速度同时也要限制不然会导致最终合成的速度方向产生变化导致在斜面上弹跳
            if (isOnSlope)
            {
                _calVelocity.y = playerRigidbody.velocity.y * _velocityWithSpeedRatio;
            }
            else
            {
                _calVelocity.y = playerRigidbody.velocity.y;
            }

            _calVelocity.z = playerRigidbody.velocity.z * _velocityWithSpeedRatio;

            playerRigidbody.velocity = _calVelocity;

            playerXozSpeed = nowMoveSpeed;
        }
    }

    // 更新玩家刚体当前在XOZ水平面的速度大小标量
    private void UpdateXozVelocity()
    {
        playerXozSpeed = Vector2.Distance(Vector2.zero,
            new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.z));
    }

    // 限制玩家垂直速度值在当前最大速度内
    public void ClampXoyVelocity()
    {
        UpdateXoyVelocity();
        if (playerXoySpeed > nowMoveSpeed)
        {
            _velocityWithSpeedRatio = nowMoveSpeed / playerXoySpeed;
            _calVelocity.x = playerRigidbody.velocity.x * _velocityWithSpeedRatio;
            _calVelocity.y = playerRigidbody.velocity.y * _velocityWithSpeedRatio;
            _calVelocity.z = playerRigidbody.velocity.z;

            playerRigidbody.velocity = _calVelocity;

            playerXoySpeed = nowMoveSpeed;
        }
    }

    // 更新玩家刚体当前在XOY水平面的速度大小标量
    private void UpdateXoyVelocity()
    {
        playerXoySpeed = Vector2.Distance(Vector2.zero,
            new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.y));
    }

    // 更新摄像机在XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度
    private void UpdateCameraForwardWithWallAbnormalAngle()
    {
        cameraForwardWithWallAbnormalAngle =
            Vector3.Angle(Vector3.ProjectOnPlane(-_forwardRaycastHit.normal, Vector3.up),
                Vector3.ProjectOnPlane(CameraController.Instance.transform.forward, Vector3.up));
    }

    // 获取玩家在斜面运动的方向
    public Vector3 GetDirectionOnSlope()
    {
        // ProjectOnPlane(a,b)函数的返回值为向量a在以向量b为法向量的平面的投影向量,又_downRaycastHit.normal为射线击中点的平面的法向量,所以这里返回的是玩家原水平运动方向在斜面上的投影的方向向量
        return Vector3.ProjectOnPlane(direction, _downRaycastHit.normal).normalized;
    }

    // 判断玩家是否在向上滑铲
    public bool CheckIsSlidingUp()
    {
        return Vector3.Angle(GetDirectionOnSlope(), Vector3.up) <= 90f;
    }

    // 获取与左或右墙壁或前相切的方向向量，即墙壁的方向向量，利用墙壁的法向量与向上的单位向量，以上两个向量的叉乘为同时垂直两向量的向量，为与墙壁这一侧面相切的向量
    public Vector3 GetWallForward()
    {
        if (hasWallOnLeft)
        {
            return Vector3.Cross(Vector3.up, _leftRaycastHit.normal);
        }

        if (hasWallOnRight)
        {
            return Vector3.Cross(Vector3.up, _rightRaycastHit.normal);
        }

        if (hasWallOnForward)
        {
            return Vector3.Cross(Vector3.up, _forwardRaycastHit.normal);
        }

        return Vector3.zero;
    }

    // 获取墙壁的法向量
    public Vector3 GetWallNormal()
    {
        if (hasWallOnForward)
        {
            return _forwardRaycastHit.normal;
        }

        if (hasWallOnLeft)
        {
            return _leftRaycastHit.normal;
        }

        if (hasWallOnRight)
        {
            return _rightRaycastHit.normal;
        }

        return Vector3.zero;
    }

    // 根据状态设置碰撞盒
    private void SetColliderByCurrentState()
    {
        if (_currentState.preState.state == E_State.Squat || _currentState.preState.state == E_State.Sliding ||
            _currentState.preState.state == E_State.Roll ||
            _currentState.state == E_State.Squat || _currentState.state == E_State.Sliding ||
            _currentState.state == E_State.Roll)
        {
            switch (_currentState.state)
            {
                case E_State.Squat:
                    squatCollider.gameObject.SetActive(true);
                    baseCollider.gameObject.SetActive(false);
                    slidingCollider.gameObject.SetActive(false);
                    break;
                case E_State.Sliding:
                    slidingCollider.gameObject.SetActive(true);
                    baseCollider.gameObject.SetActive(false);
                    squatCollider.gameObject.SetActive(false);

                    break;
                case E_State.Roll:
                    slidingCollider.gameObject.SetActive(true);
                    baseCollider.gameObject.SetActive(false);
                    squatCollider.gameObject.SetActive(false);
                    break;
                default:
                    baseCollider.gameObject.SetActive(true);
                    squatCollider.gameObject.SetActive(false);
                    slidingCollider.gameObject.SetActive(false);
                    break;
            }
        }
    }

    // 初始化碰撞盒
    private void InitCollider()
    {
        colliders.gameObject.SetActive(true);
        baseCollider.gameObject.SetActive(true);
        squatCollider.gameObject.SetActive(false);
        slidingCollider.gameObject.SetActive(false);
    }

    // 获取最后一帧脚前方有墙壁时从脚到墙壁的方向
    public Vector3 GetDirectionFootToWall()
    {
        return (_lastForwardFootRaycastHit.point - footRaycastEmissionTransform.transform.position).normalized;
    }

    // 检测人物上下空间高度是否满足目标状态的collider的切换
    private bool IsHighMatchCondition(E_State preEState, E_State nowEState)
    {
        switch (nowEState)
        {
            // 只有在前一状态使用slidingCollider时需要检测是否满足
            case E_State.Squat:
                // 向上发射射线检测上方高度
                _hasGroundInDistance = Physics.SphereCast(
                    slidingCollider.transform.position + _slidingCenterOffset,
                    squatCollider.radius * Mathf.Max(squatCollider.transform.localScale.x,
                        squatCollider.transform.localScale.z),
                    Vector3.up,
                    out _upHighRaycastHit,
                    (squatCollider.transform.position + _squatCenterOffset).y -
                    (slidingCollider.transform.position + _slidingCenterOffset).y +
                    (squatCollider.height * 0.5f * squatCollider.transform.localScale.y) -
                    (squatCollider.radius * Mathf.Max(squatCollider.transform.localScale.x,
                        squatCollider.transform.localScale.z)),
                    InfoManager.Instance.layerGroundCheck);

                // 画画
                Debug.DrawLine(
                    slidingCollider.transform.position + _slidingCenterOffset,
                    slidingCollider.transform.position + _slidingCenterOffset +
                    Vector3.up *
                    (((squatCollider.transform.position + _squatCenterOffset).y -
                      (slidingCollider.transform.position + _slidingCenterOffset).y) +
                     (squatCollider.height * 0.5f * squatCollider.transform.localScale.y)), Color.magenta);

                if (!_hasGroundInDistance && !_hasWallInDistance)
                {
                    return true;
                }

                return false;
            // 使用slidingCollider或squatCollider的状态转换为使用baseCollider的状态
            default:
                // slidingCollider -> baseCollider
                if (preEState == E_State.Sliding || preEState == E_State.Roll)
                {
                    // 向上发射射线检测上方高度
                    _hasGroundInDistance = Physics.SphereCast(
                        slidingCollider.transform.position + _slidingCenterOffset,
                        baseCollider.radius * Mathf.Max(baseCollider.transform.localScale.x,
                            baseCollider.transform.localScale.z),
                        Vector3.up,
                        out _upHighRaycastHit,
                        (baseCollider.transform.position + _baseCenterOffset).y -
                        (slidingCollider.transform.position + _slidingCenterOffset).y +
                        (baseCollider.height * 0.5f * baseCollider.transform.localScale.y) -
                        (baseCollider.radius * Mathf.Max(baseCollider.transform.localScale.x,
                            baseCollider.transform.localScale.z)),
                        InfoManager.Instance.layerGroundCheck);

                    // 画画
                    Debug.DrawLine(slidingCollider.transform.position + _slidingCenterOffset,
                        slidingCollider.transform.position + _slidingCenterOffset +
                        Vector3.up *
                        (((baseCollider.transform.position + _baseCenterOffset).y -
                          (slidingCollider.transform.position + _slidingCenterOffset).y) +
                         (baseCollider.height * 0.5f * baseCollider.transform.localScale.y)), Color.magenta);

                    if (!_hasGroundInDistance && !_hasWallInDistance)
                    {
                        return true;
                    }

                    return false;
                }

                // squatCollider -> baseCollider
                if (preEState == E_State.Squat)
                {
                    // 向上发射射线检测上方高度
                    _hasGroundInDistance = Physics.SphereCast(
                        squatCollider.transform.position + _squatCenterOffset,
                        baseCollider.radius * Mathf.Max(baseCollider.transform.localScale.x,
                            baseCollider.transform.localScale.z),
                        Vector3.up,
                        out _upHighRaycastHit,
                        (baseCollider.transform.position + _baseCenterOffset).y -
                        (squatCollider.transform.position + _squatCenterOffset).y +
                        (baseCollider.height * baseCollider.transform.localScale.y * 0.5f) -
                        (baseCollider.radius * Mathf.Max(baseCollider.transform.localScale.x,
                            baseCollider.transform.localScale.z)),
                        InfoManager.Instance.layerGroundCheck);

                    // 画画
                    Debug.DrawLine(squatCollider.transform.position + _squatCenterOffset,
                        squatCollider.transform.position + _squatCenterOffset +
                        Vector3.up *
                        (((baseCollider.transform.position + _baseCenterOffset).y -
                          (squatCollider.transform.position + _squatCenterOffset).y) +
                         (baseCollider.height * baseCollider.transform.localScale.y * 0.5f)), Color.magenta);

                    if (!_hasGroundInDistance && !_hasWallInDistance)
                    {
                        return true;
                    }

                    return false;
                }

                break;
        }

        return true;
    }
    
    // 获取当前角色向下发射最大距离的射线击中位置
    public Vector3 GetGroundPoint()
    {
        return isOnGround ? _downRaycastHit.point : _downMaxRaycastHit.point;
    }
    
    // 玩家Root四元数向目标四元数旋转
    private IEnumerator RootRotateToRootTargetRotation()
    {
        _isRotateRootToTarget = true;
        while (!isMove && !InfoManager.Instance.isLockAttackDirection)
        {
            playerAnimator.transform.rotation = Quaternion.Slerp(playerModelRootSyncPointTransform.rotation,
                _rootTargetRotation, CameraController.Instance.playerRotateSpeed).normalized;
            yield return _waitForFixedUpdate;
        }

        _isRotateRootToTarget = false;
    }

    // 动画事件----------------------
    // 从Climb状态切换为Fall状态
    public void ChangeStateClimbToFall()
    {
        ClimbState.ChangeStateClimbToFall();
    }

    public void ExitRollState()
    {
        RollState.ExitRollState();
    }

    public void InitGrappleHookTurnParameters()
    {
        GrappleState.InitGrappleHookTurnParameters();
    }
    
    public void SetNowFightState(int fightState)
    {
        FightState.SetNowFightState(fightState);
    }

    public void ExitHitState()
    {
        HitState.ExitHitState();
    }

    public void ReduceStamina()
    {
        playerCharacter.CheckStaminaValueEnableCompleteChangeState(E_State.Roll);
    }
}