using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

public class Climb : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 攀爬时间计时器
    public float _climbTimer;
    // 是否离开过地面
    private bool _hasLeftGround;
    // 是否监听玩家何时离开地面
    private bool _listenPlayerIsLeftGround;
    
    public Climb(StateMachine stateMachine) : base("Climb", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _climbTimer = 0;
        _listenPlayerIsLeftGround = false;
        InitHasLeftGround();
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
        _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.climbForce * _movementStateMachine.direction);
        
        _movementStateMachine.ClampXoyVelocity();
        
    }

    private bool ListenInputToChangeState()
    {
        _climbTimer += Time.deltaTime;

        // 监听玩家是否离开地面
        if (_listenPlayerIsLeftGround && !_movementStateMachine.isOnGround)
        {
            _listenPlayerIsLeftGround = false;
            _hasLeftGround = true;
        }
        
        // 前方已经没有墙壁已经爬上来了
        if (!_movementStateMachine.hasWallOnForward)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }
        
        // 松开前进键或达到最大攀爬时间
        if (!_movementStateMachine.MoveInputInfo.MoveForwardInput || _climbTimer >= _movementStateMachine.climbTime)
        {
            _movementStateMachine.playerRigidbody.useGravity = true;
        }
        else
        {
            CloseGravity();
        }
        
        
        // 到达地面并且之前已经从地面离开过，并且没在监听玩家是否离开地面，切换为下落模式
        if (_hasLeftGround && !_listenPlayerIsLeftGround && _movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }
        else if (!_hasLeftGround)
        {
            _hasLeftGround = true;
        }
        
        // 摄像机XOZ平面面朝向角度与面前的墙的法向量在XOZ平面的反方向角度大于最大角度切换为滑墙模式并且高度满足要求
        if (_movementStateMachine.hasWallOnForward && _movementStateMachine.cameraForwardWithWallAbnormalAngle >=
            _movementStateMachine.climbMaxAngle && _movementStateMachine.nowHigh >= _movementStateMachine.wallRunningMinHigh)
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
        if (_climbTimer < _movementStateMachine.climbTime)
        {
            _movementStateMachine.direction = _movementStateMachine.playerTransform.up;
            _movementStateMachine.direction += _movementStateMachine.playerTransform.right *
                                               _movementStateMachine.MoveInputInfo.HorizontalInput;
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

    private void InitHasLeftGround()
    {
        _hasLeftGround = false;
        switch (preState.name)
        {
            case "Jump":
                _hasLeftGround = true;
                break;
            case "Fall":
                _hasLeftGround = true;
                break;
            case "WallRunning":
                _hasLeftGround = true;
                break;
            default:
                _listenPlayerIsLeftGround = true;
                break;
        }
    }
}
