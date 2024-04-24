using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

public class Climb : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 攀爬时间计时器
    public float ClimbTimer;
    // 向左右离开墙壁后结束攀爬状态延迟时间计时器(为了区分向上到顶点切换攀爬状态)
    private float _leftClimbStateTimer;
    // 是否监听玩家离开地面
    private bool _listenPlayerIsLeftGround;
    // 是否离开过地面
    private bool _hasLeftGround;
    // 是否给予玩家向前的速度
    private bool _isGiveForwardVelocity;
    // 是否是向上攀爬离开墙体
    private bool _isLeftClimbStateByUp;
    // 是否给予玩家向上攀爬的力
    private bool _isGiveClimbForce;
    // 用来保存玩家向上攀爬脚离开墙最后一帧的Xoz平面方向,通过该方向获取的速度
    private Vector3 _xozVelocity;
    // 是否已经进行过向上攀爬的参数修改
    private bool _isFixedLeftClimbStateByUpParameters;
    
    public Climb(StateMachine stateMachine) : base(E_State.Climb, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        
        InitParameters();

        // 关闭重力
        CloseGravity();
    }

    public override void Exit()
    {
        base.Exit();
        
        // 打开重力
        _movementStateMachine.playerRigidbody.useGravity = true;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (ListenInputToChangeState())
        {
            return;
        }

        UpdateDirectionWithSpeed();
    }

    public override void UpdatePhysic()
    {
        {        if (_isGiveClimbForce)

            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.climbForce * _movementStateMachine.direction);
        }

        if (_isGiveForwardVelocity && _movementStateMachine.playerRigidbody.velocity != _xozVelocity)
        {
            _movementStateMachine.playerRigidbody.velocity = _xozVelocity;
        }
        
        _movementStateMachine.ClampXoyVelocity();
    }

    private bool ListenInputToChangeState()
    {
        if (!_isLeftClimbStateByUp)
        {
            ClimbTimer += Time.deltaTime;
        }
        
        // 监听玩家是否离开地面
        if (_listenPlayerIsLeftGround && !_movementStateMachine.isOnGround)
        {
            _listenPlayerIsLeftGround = false;
            _hasLeftGround = true;
        }
        
        // 松开前进键或达到最大攀爬时间
        if (!_movementStateMachine.MoveInputInfo.MoveForwardInput || ClimbTimer >= _movementStateMachine.climbTime)
        {
            _movementStateMachine.playerRigidbody.useGravity = true;
        }
        else
        {
            CloseGravity();
        }
        
        // 到达地面并且之前已经从地面离开过，并且没在监听玩家是否离开地面，或正在监听玩家是否离开地面但是松开前进键，切换为下落模式
        if ((_hasLeftGround && !_listenPlayerIsLeftGround && _movementStateMachine.isOnGround) ||
            (_movementStateMachine.isOnGround && _movementStateMachine.MoveInputInfo.VerticalInput != 1))
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }
        else if (!_hasLeftGround)
        {
            _hasLeftGround = true;
        }
        
        // 脚前方有墙壁并且身体前方没有墙壁
        if (!_movementStateMachine.hasWallOnForward && 
            !_movementStateMachine.hasWallOnHeadForward && 
            _movementStateMachine.hasWallOnFootForward && 
            _movementStateMachine.MoveInputInfo.VerticalInput == 1)
        {
            _isLeftClimbStateByUp = true;
        }
        
        // 向左或右移动离开墙壁切换为下落状态
        if (!_movementStateMachine.hasWallOnForward && 
            !_movementStateMachine.hasWallOnHeadForward &&
            !_movementStateMachine.hasWallOnFootForward &&
            !_isLeftClimbStateByUp)
        {
            if (_leftClimbStateTimer < 0.1f)
            {
                _leftClimbStateTimer += Time.deltaTime;
            }
            else
            {
                _movementStateMachine.ChangeState(_movementStateMachine.FallState);
                return true;
            }
        }
        
        // 向上离开墙壁
        if(!_movementStateMachine.hasWallOnForward && 
           !_movementStateMachine.hasWallOnHeadForward &&
           !_movementStateMachine.hasWallOnFootForward &&
           _isLeftClimbStateByUp && 
           !_isFixedLeftClimbStateByUpParameters)
        {
            // 给与玩家向前推的力让玩家能够到达墙顶
            _isGiveForwardVelocity = true;
            // 启用向下进行球形射线检测
            _movementStateMachine.isUseSphereCast = true;
            // 不能快速进入跑步状态
            _movementStateMachine.isFastToRun = false;

            // 去向给予玩家向上和向左右的力
            _isGiveClimbForce = false;
            // 将玩家坐标向上移动一部分
            // _movementStateMachine.playerTransform.position += Vector3.up;
            // 给予一个固定向前移动的速度
            _xozVelocity = _movementStateMachine.GetDirectionFootToWall() * 2f;
            _isFixedLeftClimbStateByUpParameters = true;
        }
        
        // 没在地面且在攀爬状态进行跳跃
        if (!_movementStateMachine.isOnGround && _movementStateMachine.MoveInputInfo.JumpInput)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.JumpState);
            return true;
        }
        
        // 摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度切换为滑墙模式并且高度满足要求,并且在攀爬状态经过一段时间
        if (_movementStateMachine.hasWallOnForward && _movementStateMachine.cameraForwardWithWallAbnormalAngle >
            _movementStateMachine.climbMaxAngle &&
            _movementStateMachine.nowHigh >= _movementStateMachine.wallRunningMinHigh &&
            ClimbTimer > 0.1f)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.WallRunningState);
            return true;
        }
        
        return false;
    }

    private void UpdateDirectionWithSpeed()
    {
        // 更新移动方向
        // 若是攀爬时间还没到最大攀爬时间则还可以往上爬不然只能左右稍微改变
        if (ClimbTimer < _movementStateMachine.climbTime)
        {
            _movementStateMachine.direction = _movementStateMachine.playerTransform.up;
            _movementStateMachine.direction += 0.5f *
                                               _movementStateMachine.MoveInputInfo.HorizontalInput *
                                               _movementStateMachine.playerTransform.right;
            _movementStateMachine.direction = _movementStateMachine.direction.normalized;
        }
        else
        {
            _movementStateMachine.direction = (_movementStateMachine.playerTransform.right *
                                               _movementStateMachine.MoveInputInfo.HorizontalInput).normalized;
        }
        // 更新当前方向移动速度(有水平方向移动的输入则以水平速度为主)
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0)
        {
            // 更新当前速度为水平速度
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.climbHorizontalSpeed;
        }
        // 保持移动
        else
        {
            // 更新当前速度为向上攀爬速度
            _movementStateMachine.nowMoveSpeed = _movementStateMachine.climbUpSpeed;
        }
    }
    
    private void CloseGravity()
    {
        _movementStateMachine.playerRigidbody.useGravity = false;
        // 清空Y轴向下速度
        if (_movementStateMachine.playerRigidbody.velocity.y < 0)
        {
            _movementStateMachine.playerRigidbody.velocity = new Vector3(_movementStateMachine.playerRigidbody.velocity.x,
                0, _movementStateMachine.playerRigidbody.velocity.z);
        }
    }
    
    // 初始化参数
    private void InitParameters()
    {
        ClimbTimer = 0;

        InitHasLeftGround();
        _leftClimbStateTimer = 0;

        _isLeftClimbStateByUp = false;
        _isGiveForwardVelocity = false;
        _isGiveClimbForce = true;
        _isFixedLeftClimbStateByUpParameters = false;
        _xozVelocity = Vector3.zero;
    }

    // 初始化是否离开过地面，若未离开过地面则开启监听是否离开地面
    private void InitHasLeftGround()
    {
        _hasLeftGround = false;
        switch (preState.state)
        {
            case E_State.Jump:
                _hasLeftGround = true;
                _listenPlayerIsLeftGround = false;
                break;
            case E_State.Fall:
                _hasLeftGround = true;
                _listenPlayerIsLeftGround = false;
                break;
            case E_State.WallRunning:
                _hasLeftGround = true;
                _listenPlayerIsLeftGround = false;
                break;
            default:
                _listenPlayerIsLeftGround = true;
                break;
        }
    }

    // 设置最大向上速度
    private void SetMaxUpVelocity()
    {
        _movementStateMachine.playerRigidbody.velocity = new Vector3(_movementStateMachine.playerRigidbody.velocity.x,
            _movementStateMachine.climbUpSpeed, _movementStateMachine.playerRigidbody.velocity.z);
    }

    public void ChangeStateClimbToFall()
    {
        _movementStateMachine.isUseSphereCast = true;
        _movementStateMachine.isFastToRun = false;
        _movementStateMachine.ChangeState(_movementStateMachine.FallState);
    }
}
