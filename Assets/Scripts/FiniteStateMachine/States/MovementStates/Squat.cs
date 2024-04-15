using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squat : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;

    public Squat(StateMachine stateMachine) : base("Squat", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();

        // 将玩家模型压缩一半达到蹲下效果，之后导入模型和动作后改成播放对应动作，这里要改掉
        _movementStateMachine.transform.localScale = new Vector3(_movementStateMachine.transform.localScale.x,
            _movementStateMachine.squatYScale,
            _movementStateMachine.transform.localScale.z);
        
        // 让玩家贴地
        // _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.squatMoveForce * 30f * Vector3.down);
        _movementStateMachine.playerTransform.position -= new Vector3(0,
            _movementStateMachine.playerHeight * (1f - _movementStateMachine.slidingYScale) * 0.5f, 0);
    }

    public override void Exit()
    {
        base.Exit();

        // 让玩家远离地面
        // _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.squatMoveForce * Vector3.up);
        _movementStateMachine.playerTransform.position += new Vector3(0,
            _movementStateMachine.playerHeight * (1f - _movementStateMachine.slidingYScale) * 0.5f, 0);

        // 将玩家模型还原，之后导入模型和动作后改成播放对应动作，这里要改掉
        _movementStateMachine.transform.localScale = new Vector3(_movementStateMachine.transform.localScale.x,
            1f,
            _movementStateMachine.transform.localScale.z);
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
        base.UpdatePhysic();
        
        // 根据移动方向移动
        // Translate移动
        // _movementStateMachine.playerTransform.Translate(_movementStateMachine.nowMoveSpeed * Time.deltaTime * _movementStateMachine.direction,Space.World);
        if (!_movementStateMachine.isOnSlope)
        {
            // 通过给力移动
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.squatMoveForce *
                _movementStateMachine.direction, ForceMode.Force);
        }
        else
        {
            _movementStateMachine.playerRigidbody.AddForce(
                _movementStateMachine.nowMoveSpeed * _movementStateMachine.squatMoveForce *
                _movementStateMachine.GetDirectionOnSlope(), ForceMode.Force);
        }
        // 限制玩家最大速度
        _movementStateMachine.ClampXozVelocity();
    }

    private bool ListenInputToChangeState()
    {
        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
            return true;
        }
        
        // 松开WASD或摁住WS或摁住AD或摁住WASD并且松开下蹲键
        if (_movementStateMachine.MoveInputInfo.HorizontalInput == 0 &&
            _movementStateMachine.MoveInputInfo.VerticalInput == 0 &&
            !_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.IdleState);
            return true;
        }
        // 摁了移动键但是松开下蹲键
        else if (!_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.WalkState);
            return true;
        }

        return false;
    }

    private void UpdateDirectionWithSpeed()
    {
        // 更新移动方向
        _movementStateMachine.direction = _movementStateMachine.playerTransform.forward *
                                          _movementStateMachine.MoveInputInfo.VerticalInput;
        _movementStateMachine.direction += _movementStateMachine.playerTransform.right *
                                           _movementStateMachine.MoveInputInfo.HorizontalInput;
        _movementStateMachine.direction = _movementStateMachine.direction.normalized;
            
        // 更新速度
        _movementStateMachine.nowMoveSpeed = _movementStateMachine.squatSpeed;
    }
}