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
    // 当前斜面角度
    [HideInInspector] public float slopeAngle;
    // 摄像机在XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度
    [HideInInspector] public float cameraForwardWithWallAbnormalAngle;
    // 在下落、跳跃后能否快速进入奔跑状态
    [HideInInspector] public bool isFastToRun;
    // 当前玩家距离地面高度
    [HideInInspector] public float nowHigh;

    [Header("玩家及场景参数")] // 以后考虑是不是要换到别的脚本里面去
    // 玩家高度
    public float playerHeight = 2f;
    // 允许玩家运动的最大斜面角度
    public float maxSlopeAngle = 45f;

    [Header("玩家运动相关参数")]
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

    [Header("奔跑")]
    // 奔跑
    // 跑步向前的速度的最大值
    public float runForwardSpeed = 20f;
    // 玩家从WalkToRun所需摁键时间
    public float toRunTime = 0.5f;
    // 奔跑时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = runMoveForce * nowMoveSpeed)
    public float runMoveForce = 20f;

    [Header("下蹲")]
    // 下蹲
    // 下蹲时移动的速度的最大值
    public float squatSpeed = 5f;
    // 下蹲时玩家缩放系数
    public float squatYScale = 0.5f;
    // 下蹲时给予玩家力的大小
    public float squatMoveForce = 8f;

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
    // 滑铲时玩家缩放系数
    public float slidingYScale = 0.2f;
    // 滑铲冷却时间(进入Run状态后需要过一段时间后才能进行滑铲)
    public float slidingCoolTime = 0.2f;
    
    [Header("下落")]
    // 下落
    // 快速下落时所受重力倍数
    public float fallGravityScale = 3f;
    // 下落时水平移动的最大速度
    public float fallSpeed = 1f;
    
    [Header("滑墙")]
    // 滑墙
    // 可以进行滑墙的最低高度
    public float wallRunningMinHigh = 0.5f;
    // 在墙壁上向前滑行的最大速度
    public float wallRunningForwardSpeed = 8f;
    // 在墙壁上下滑行的最大速度
    public float wallRunningVertical = 3f;
    // 在墙上滑行时驱动玩家移动力的大小
    public float wallRunningForce = 6f;
    // 在墙上滑行的最大时间
    public float wallRunningTime = 3f;
    // 检查左右两边是否有可以进行滑墙的墙的距离
    public float wallCheckDistance = 1f;
    
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
    
    // 组件
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody playerRigidbody;

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
        WallRunningState = new WallRunning(this);
        ClimbState = new Climb(this);

        // 获取组件
        playerTransform = transform;
        playerRigidbody = GetComponent<Rigidbody>();

        // 初始化参数
        InitParameters();
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
        
        UpdateXozVelocity();
        
        base.Update();
        
        
    }

    // 重写父类FixUpdate函数在玩家处于斜面上时给予玩家手动重力
    protected override void FixedUpdate()
    {
        // 除了滑墙和攀爬重力全部在这里管理
        if (isOnSlope)
        {
            // 关闭玩家刚体自带的重力
            playerRigidbody.useGravity = false;
            switch (_currentState.name)
            {
                case "Walk":
                case "Run":
                case "Squat":
                case "Sliding":
                    playerRigidbody.AddForce(playerRigidbody.mass * 9.18f * Vector3
                        .ProjectOnPlane(Vector3.down, _downRaycastHit.normal)
                        .normalized);
                    break;
            }
        }
        else if(_currentState.name != "WallRunning" && _currentState.name != "Climb")
        {
            playerRigidbody.useGravity = true;
        }

        base.FixedUpdate();
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
            playerTransform.position + playerTransform.forward * wallCheckDistance, Color.yellow);
        
        
        // 方向
        Debug.DrawLine(playerTransform.position, playerTransform.position + direction * 10, Color.white);
    }

    protected override BaseState GetInitialState()
    {
        return IdleState;
    }

    // 初始化参数
    private void InitParameters()
    {
        UpdateIsOnGroundAndIsOnSlopeAndNowHigh();

        nowMoveSpeed = 0;
    }

    // 更新移动的输入信息(前1后-1，左-1,右1);
    private void UpdateMoveInputInformation()
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
    private void UpdateIsOnGroundAndIsOnSlopeAndNowHigh()
    {
        if (!(InfoManager.Instance is null))
        {
            // 向下发射射线
            isOnGround = Physics.Raycast(playerTransform.position, Vector3.down, out _downRaycastHit,
                playerHeight * 0.5f + heightOffset, InfoManager.Instance.layerGround);
            
            // 在地面上
            if (isOnGround)
            {
                // 更新玩家当前距离地面高度
                nowHigh = Vector3.Distance(playerTransform.position, _downRaycastHit.point);
                // 更新玩家刚体阻力
                playerRigidbody.drag = InfoManager.Instance.groundDrag + InfoManager.Instance.airDrag;
                // 更新玩家当前是否在可运动角度范围内的斜面上
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
            // 没在地面上(向下的射线未射到地面)
            else
            {
                nowHigh = Physics.Raycast(playerTransform.position, Vector3.down, out _downMaxRaycastHit,
                    InfoManager.Instance.maxHigh, InfoManager.Instance.layerGround)
                    ? Vector3.Distance(_downMaxRaycastHit.point, playerTransform.position)
                    : InfoManager.Instance.maxHigh;
                isOnSlope = false;
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
            InfoManager.Instance.layerWall);
        
        // 向右发射射线检测是否存在墙壁
        hasWallOnRight = Physics.Raycast(playerTransform.position, playerTransform.right, out _rightRaycastHit,
            wallCheckDistance,
            InfoManager.Instance.layerWall);
    }

    // 向前方发射射线检测是否存在墙壁
    private void UpdateHasWallOnForward()
    {
        // 向前发射射线检测是否存在墙壁
        hasWallOnForward = Physics.Raycast(playerTransform.position, playerTransform.forward, out _forwardRaycastHit,
            wallCheckDistance, InfoManager.Instance.layerWall);
        
        // 当前前方存在墙壁时更新计算cameraForwardWithWallAbnormalAngle
        if (hasWallOnForward)
        {
            UpdateCameraForwardWithWallAbnormalAngle();
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
            _calVelocity = playerRigidbody.velocity.normalized * nowMoveSpeed;
            playerXozSpeed = nowMoveSpeed;
            playerRigidbody.velocity = new Vector3(_calVelocity.x, playerRigidbody.velocity.y, _calVelocity.z);
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
            _calVelocity = playerRigidbody.velocity.normalized * nowMoveSpeed;
            playerXoySpeed = nowMoveSpeed;
            playerRigidbody.velocity = new Vector3(_calVelocity.x, _calVelocity.y, playerRigidbody.velocity.z);
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
                Vector3.ProjectOnPlane(CameraController.Instance.transform.forward,Vector3.up));
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
    
    // 获取抵消自己加上的斜面重力
    public Vector3 GetOffsetGravityOnSlope()
    {
        return -10f * playerRigidbody.mass * Vector3.ProjectOnPlane(Vector3.down, _downRaycastHit.normal).normalized;
    }
    
    // 获取与左或右墙壁或前相切的方向向量，即墙壁的方向向量，利用墙壁的法向量与向上的单位向量，以上两个向量的叉乘为同时垂直两向量的向量，为与墙壁这一侧面相切的向量
    public Vector3 GetWallForward()
    {
        if (hasWallOnLeft)
        {
            return Vector3.Cross(Vector3.up,_leftRaycastHit.normal);
        }
        if (hasWallOnRight)
        {
            return Vector3.Cross(Vector3.up,_rightRaycastHit.normal);
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
    
    // 获取下方射线与地面击中点与玩家坐标位置的距离
    public float GetDownHitWithPlayerDistance()
    {
        return Vector3.Distance(_downRaycastHit.point, playerTransform.position);
    }
}