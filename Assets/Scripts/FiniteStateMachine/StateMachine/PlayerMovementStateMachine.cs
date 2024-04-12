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
    
    // 移动输入信息
    [HideInInspector] public MoveInputInformation MoveInputInfo;
    
    // 当前水平移动移动速度的最大值
    [HideInInspector] public float nowMoveSpeed;
    // 当前玩家是否在地面上
    [HideInInspector] public bool isOnGround;
    // 当前玩家刚体在XOZ平面上移动速度大小
    [HideInInspector] public float playerXozSpeed;
    
    // 当前状态
    [HideInInspector] public BaseState CurrentState => _currentState;
    // 前一状态
    [HideInInspector] public BaseState PreState;

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
    public float walkMoveForce = 2.5f;

    // 跳跃
    // 跳跃瞬间给玩家的力的大小
    public float jumpForce = 10f;
    // 玩家是否在地面的高度检测时的偏移量
    public float heightOffset = 0.2f;

    // 奔跑
    // 玩家从WalkToRun所需摁键时间
    public float toRunTime;
    // 奔跑时给予玩家力的相对大小(与上面的限制玩家最大速度成正比?目前是这样实现，为玩家加力时力的大小 = runMoveForce * nowMoveSpeed)
    public float runMoveForce = 2.5f;
    
    // 组件
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody playerRigidbody;

    // 计算Xoz平面玩家刚体速度用的临时变量
    private Vector3 _calXozVelocity;
    private void Awake()
    {
        // 初始化状态
        IdleState = new Idle(this);
        WalkState = new Walk(this);
        JumpState = new Jump(this);
        FallState = new Fall(this);
        RunState = new Run(this);

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
        UpdateXozVelocity();
    }

    protected override BaseState GetInitialState()
    {
        return IdleState;
    }

    // 初始化参数
    private void InitParameters()
    {
        UpdateIsOnGround();
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
    }

    // 更新当前是否处于地面
    public void UpdateIsOnGround()
    {
        if (!(InfoManager.Instance is null))
        {
            isOnGround = Physics.Raycast(playerTransform.position, Vector3.down, playerHeight * 0.5f + heightOffset, InfoManager.Instance.layerGround);
            Debug.DrawLine(playerTransform.position, playerTransform.position + Vector3.down * (playerHeight * 0.5f + heightOffset), Color.red);
            
            // 更新玩家刚体的阻力大小
            if (isOnGround)
            {
                playerRigidbody.drag = InfoManager.Instance.groundDrag + InfoManager.Instance.airDrag;
            }
            else
            {
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
    
    // 获取玩家刚体当前在XOZ水平面的速度大小标量
    public void UpdateXozVelocity()
    {
        playerXozSpeed = Vector2.Distance(Vector2.zero, new Vector2(playerRigidbody.velocity.x, playerRigidbody.velocity.z));
    }
}