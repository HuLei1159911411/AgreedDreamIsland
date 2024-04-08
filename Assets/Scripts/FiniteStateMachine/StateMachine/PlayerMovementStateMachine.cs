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
    
    // 移动输入信息
    [HideInInspector] public MoveInputInformation MoveInputInfo;
    
    // 当前水平移动移动速度
    [HideInInspector] public float nowMoveSpeed;
    
    // 当前玩家是否在地面上
    [HideInInspector] public bool isOnGround;

    
    // 当前状态
    [HideInInspector] public BaseState CurrentState => _currentState;

    [Header("玩家参数")] // 以后考虑是不是要换到别的脚本里面去
    // 玩家高度
    public float playerHeight = 2f;

    [Header("玩家运动相关参数")]
    // 移动
    // 走路向前的速度
    public float walkForwardSpeed = 8f;

    // 走路向后的速度
    public float walkBackwardSpeed = 5f;

    // 跑步向前的速度
    public float runForwardSpeed = 20f;

    // 走路水平方向移动的速度
    public float walkHorizontalSpeed = 5f;

    // 跳跃
    // 跳跃瞬间给玩家的力的大小
    public float jumpForce = 10f;

    // 玩家是否在地面的高度检测时的偏移量
    public float heightOffset = 0.2f;

    // 地面所在层
    public LayerMask layerGround;

    // 组件
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody playerRigidbody;

    private void Awake()
    {
        IdleState = new Idle(this);
        WalkState = new Walk(this);
        JumpState = new Jump(this);
        FallState = new Fall(this);

        playerTransform = transform;
        playerRigidbody = GetComponent<Rigidbody>();

        InitParameters();
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
    }

    // 更新当前
    public void UpdateIsOnGround()
    {
        isOnGround = Physics.Raycast(playerTransform.position, Vector3.down, playerHeight * 0.5f + heightOffset, layerGround);
        Debug.DrawLine(playerTransform.position, playerTransform.position + Vector3.down * (playerHeight * 0.5f + heightOffset), Color.red);
    }
}