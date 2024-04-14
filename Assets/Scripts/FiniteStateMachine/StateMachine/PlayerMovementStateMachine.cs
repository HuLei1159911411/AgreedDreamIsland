using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
}

public class PlayerMovementStateMachine : StateMachine
{
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

    // 移动输入信息
    [HideInInspector] public MoveInputInformation MoveInputInfo;
    // 当前水平移动移动速度的最大值
    public float nowMoveSpeed;
    // 当前玩家是否在地面上
    [HideInInspector] public bool isOnGround;
    // 当前玩家刚体在XOZ平面上移动速度大小
    [HideInInspector] public float playerXozSpeed;
    // 当前玩家是否在可运动角度范围内的斜面上
    [HideInInspector] public bool isOnSlope;
    // 玩家水平移动的方向
    [HideInInspector] public Vector3 direction;
    // 当前状态
    [HideInInspector] public BaseState CurrentState => _currentState;
    // 前一状态
    [HideInInspector] public BaseState PreState;
    // 当前斜面角度
    public  float slopeAngle;

    [Header("玩家参数")] // 以后考虑是不是要换到别的脚本里面去
    // 玩家高度
    public float playerHeight = 2f;

    [Header("玩家运动相关参数")]
    // 移动
    // 走路向前的速度的最大值
    public float walkForwardSpeed = 8f;
    // 走路向后的速度的最大值
    public float walkBackwardSpeed = 5f;
    // 跑步向前的速度的最大值
    public float runForwardSpeed = 20f;
    // 走路水平方向移动的速度的最大值
    public float walkHorizontalSpeed = 5f;
    // 走路时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = walkMoveForce * nowMoveSpeed)
    public float walkMoveForce = 10f;

    // 跳跃
    // 是否通过给力让玩家跳跃
    public bool jumpByForce;
    // 跳跃瞬间给玩家的力的大小
    public float jumpForce = 10f;
    // 跳跃瞬间给玩家的速度的大小
    public float jumpVelocity = 10f;
    // 玩家是否在地面的高度检测时的偏移量
    public float heightOffset = 0.2f;

    // 奔跑
    // 玩家从WalkToRun所需摁键时间
    public float toRunTime;
    // 奔跑时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = runMoveForce * nowMoveSpeed)
    public float runMoveForce = 20f;

    // 下蹲
    // 下蹲时移动的速度的最大值
    public float squatSpeed = 5f;
    // 下蹲时玩家缩放系数
    public float squatYScale = 0.5f;
    // 下蹲时给予玩家力的大小
    public float squatMoveForce = 8f;
    
    // 允许玩家运动的最大斜面角度
    public float maxSlopeAngle = 45f;

    // 滑铲
    // 平地上滑铲速度最大速度
    public float slidingSpeed = 12f;
    // 斜面上滑铲相对最大速度(当斜面越陡时速度越大)
    public float slidingOnSlopeSpeed = 12f;
    // 滑铲给玩家力的大小
    public float slidingMoveForce = 30f;
    // 滑铲加速时间
    public float slidingAccelerateTime = 0.5f;
    // 滑铲时玩家缩放系数
    public float slidingYScale = 0.2f;
    
    // 组件
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody playerRigidbody;

    // 计算Xoz平面玩家刚体速度用的临时变量
    private Vector3 _calXozVelocity;
    // 由玩家向下发射的检测地面与斜面用的射线的击中信息
    private RaycastHit _downRaycastHit;
    
    private void Awake()
    {
        // 初始化状态
        IdleState = new Idle(this);
        WalkState = new Walk(this);
        JumpState = new Jump(this);
        FallState = new Fall(this);
        RunState = new Run(this);
        SquatState = new Squat(this);
        SlidingState = new Sliding(this);

        // 获取组件
        playerTransform = transform;
        playerRigidbody = GetComponent<Rigidbody>();

        // 初始化参数
        InitParameters();
    }

    // 重写父类Update函数统一进行一些数据更新
    protected override void Update()
    {
        base.Update();
        
        // 更新移动输入信息
        UpdateMoveInputInformation();

        // 更新检查是否在地面
        UpdateIsOnGroundWithIsOnSlope();
        
        UpdateXozVelocity();
    }

    // 重写父类FixUpdate函数在玩家处于斜面上时给予玩家手动重力
    protected override void FixedUpdate()
    {
        if (isOnSlope)
        {
            // 关闭玩家刚体自带的重力
            playerRigidbody.useGravity = false;
        }
        else
        {
            playerRigidbody.useGravity = true;
        }

        base.FixedUpdate();
    }

    protected override BaseState GetInitialState()
    {
        return IdleState;
    }

    // 初始化参数
    private void InitParameters()
    {
        UpdateIsOnGroundWithIsOnSlope();

        nowMoveSpeed = 0;
    }

    // 更新移动的输入信息
    public void UpdateMoveInputInformation()
    {
        MoveInputInfo.MoveForwardInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]);
        MoveInputInfo.MoveBackwardInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveBackward]);
        MoveInputInfo.MoveLeftInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]);
        MoveInputInfo.MoveRightInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]);
        MoveInputInfo.VerticalInput =
            (MoveInputInfo.MoveForwardInput ? 1 : 0) + (MoveInputInfo.MoveBackwardInput ? -1 : 0);
        MoveInputInfo.HorizontalInput = (MoveInputInfo.MoveRightInput ? 1 : 0) + (MoveInputInfo.MoveLeftInput ? -1 : 0);

        MoveInputInfo.JumpInput = Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Jump]);
        MoveInputInfo.RunInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Run]);
        MoveInputInfo.SquatInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Squat]);
        MoveInputInfo.SlidingInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Sliding]);
    }

    // 更新当前是否处于地面与玩家当前是否在可运动角度范围内的斜面上
    public void UpdateIsOnGroundWithIsOnSlope()
    {
        if (!(InfoManager.Instance is null))
        {
            isOnGround = Physics.Raycast(playerTransform.position, Vector3.down, out _downRaycastHit,
                playerHeight * 0.5f + heightOffset, InfoManager.Instance.layerGround);
            Debug.DrawLine(playerTransform.position,
                playerTransform.position + Vector3.down * (playerHeight * 0.5f + heightOffset), Color.red);

            // 更新玩家刚体的阻力大小
            if (isOnGround)
            {
                playerRigidbody.drag = InfoManager.Instance.groundDrag + InfoManager.Instance.airDrag;
                // 更新玩家当前是否在可运动角度范围内的斜面上
                // 计算当前斜面的角度,_downRaycastHit.normal是击中点的面法线向量,自己想一下,通过角度转换就会得到这个角度就是斜面的斜率
                slopeAngle = Vector3.Angle(Vector3.up, _downRaycastHit.normal);
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    isOnSlope = true;
                }
                else
                {
                    isOnSlope = false;
                }
            }
            else
            {
                isOnSlope = false;
                isOnSlope = false;
                playerRigidbody.drag = InfoManager.Instance.airDrag;
            }
        }
    }

    // 获取当前状态的状态名(状态名为状态类内部成员参数string name)
    public string GetNowState()
    {
        return _currentState.name;
    }

    // 限制玩家水平速度值在当前最大速度内
    public void ClampXozVelocity()
    {
        UpdateXozVelocity();
        if (playerXozSpeed > nowMoveSpeed)
        {
            _calXozVelocity = playerRigidbody.velocity.normalized * nowMoveSpeed;
            playerXozSpeed = nowMoveSpeed;
            playerRigidbody.velocity = new Vector3(_calXozVelocity.x, playerRigidbody.velocity.y, _calXozVelocity.z);
        }
    }

    // 更新玩家刚体当前在XOZ水平面的速度大小标量
    public void UpdateXozVelocity()
    {
        playerXozSpeed = Vector2.Distance(Vector2.zero,
            new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.z));
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
    
}