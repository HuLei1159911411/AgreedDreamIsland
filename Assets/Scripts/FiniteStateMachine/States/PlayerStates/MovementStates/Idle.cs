using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class Idle : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 冷却计时器
    private float _coolTimeTimer;
    // 离开地面时间计时器
    private float _leafGroundTimer;
    public Idle(StateMachine stateMachine) : base(E_State.Idle, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
            preState = _movementStateMachine.GetInitialState();
        }
    }

    public override void Enter()
    {
        base.Enter();

        _coolTimeTimer = 0f;
        _movementStateMachine.isFastToRun = false;
        _movementStateMachine.playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation |
                                                            RigidbodyConstraints.FreezePositionX | 
                                                            RigidbodyConstraints.FreezePositionZ;
        _leafGroundTimer = 0f;
    }

    public override void Exit()
    {
        base.Exit();
        _movementStateMachine.playerRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        _coolTimeTimer += Time.deltaTime;
        
        // 不在地面
        if (!_movementStateMachine.isOnGround)
        {
            _leafGroundTimer += Time.deltaTime;
            if (_leafGroundTimer >= 0.5f)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.FallState);
                return;
            }
        }
        else
        {
            _leafGroundTimer = 0f;
        }
        
        // 前一状态为下落并且快速下落
        if (!(preState is null) && preState.state == E_State.Fall && (preState as Fall).isFastFall && _coolTimeTimer < 0.5f)
        {
            return;
        }
        
        // 摁移动键
        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0 || _movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            // 摁前进键加空格并且前方有墙壁并且角度满足条件
            if (_movementStateMachine.MoveInputInfo.MoveForwardInput && 
                _movementStateMachine.MoveInputInfo.JumpInput &&
                _movementStateMachine.hasWallOnForward &&
                _movementStateMachine.cameraForwardWithWallAbnormalAngle <= _movementStateMachine.climbMaxAngle)
            {
                stateMachine.ChangeState(_movementStateMachine.ClimbState);
                return;
            }
            stateMachine.ChangeState(_movementStateMachine.WalkState);
            return;
        }
        // 摁跳跃键
        if (_movementStateMachine.MoveInputInfo.JumpInput && _coolTimeTimer > 0.2f)
        {
            stateMachine.ChangeState(_movementStateMachine.JumpState);
            return;
        }
        // 摁跑步键
        if (_movementStateMachine.MoveInputInfo.RollInput && _coolTimeTimer > 0.1f)
        {
            stateMachine.ChangeState(_movementStateMachine.RollState);
            return;
        }
        // 摁下蹲键
        if (_movementStateMachine.MoveInputInfo.SquatInput)
        {
            stateMachine.ChangeState(_movementStateMachine.SquatState);
        }
    }
}
