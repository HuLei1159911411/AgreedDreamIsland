using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    public Death(StateMachine stateMachine) : base(E_State.Death, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }
    }
    
    public override void Enter()
    {
        base.Enter();
        CameraController.Instance.isStopPlayerRotation = true;
        _movementStateMachine.playerRigidbody.useGravity = false;
        _movementStateMachine.playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _movementStateMachine.playerRigidbody.velocity = Vector3.zero;
        _movementStateMachine.playerAnimator.SetTrigger(_movementStateMachine.DicAnimatorIndexes["ToDeath"]);
        _movementStateMachine.colliders.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
