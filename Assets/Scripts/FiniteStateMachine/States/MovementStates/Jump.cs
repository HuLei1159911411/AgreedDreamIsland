using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Jump : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    private Vector3 _velocity;
    
    public Jump(StateMachine stateMachine) : base("Jump", stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }

    public override void Enter()
    {
        base.Enter();
        _movementStateMachine.isOnGround = false;
        _movementStateMachine.isOnSlope = false;
        DoJump();
    }

    public override void Exit()
    {
        base.Exit();
        
        // Jump没有将前一状态设置为自己为了让Fall状态知道前一状态是什么
    }

    public override void UpdateLogic()
    {
        base.UpdatePhysic();
        // 当向上速度小于等于0时自动转换为Fall状态

        _velocity = _movementStateMachine.playerRigidbody.velocity;
        if (_velocity.y <= 0)
        {
            _movementStateMachine.ChangeState(_movementStateMachine.FallState);
        }
    }

    private void DoJump()
    {
        if (_movementStateMachine.jumpByForce)
        {
            _velocity = _movementStateMachine.playerRigidbody.velocity;
            // 清空角色垂直方向上的速度
            _movementStateMachine.playerRigidbody.velocity = new Vector3(_velocity.x, 0f, _velocity.z);
            // 为角色增加一个向上的力
            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.playerTransform.up * _movementStateMachine.jumpForce, ForceMode.Impulse);
        }
        else
        {
            // 为角色增加一个向上的速度
            _movementStateMachine.playerRigidbody.velocity += new Vector3(0, _movementStateMachine.jumpVelocity, 0);
        }
    }
}
