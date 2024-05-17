using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Death : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    private float _timer;
    private bool hasInit;
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
        _timer = 0f;
        hasInit = false;
        CameraController.Instance.isStopPlayerRotation = true;
        _movementStateMachine.playerRigidbody.useGravity = false;
        _movementStateMachine.playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        _movementStateMachine.playerRigidbody.velocity = Vector3.zero;
        _movementStateMachine.playerAnimator.SetTrigger(_movementStateMachine.DicAnimatorIndexes["ToDeath"]);
        _movementStateMachine.colliders.gameObject.SetActive(false);
    }

    public override void UpdateLogic()
    {
        _timer += Time.deltaTime;
        if (_timer >= 5f && !hasInit)
        {
            hasInit = true;
            _movementStateMachine.playerTransform.position = GuidingPointsGroup.Instance.GetRecentPoint() + (_movementStateMachine.baseCollider.height * Vector3.up);
            _movementStateMachine.Init();
            _movementStateMachine.playerCharacter.Init();
            CameraController.Instance.Init();
        }
    }

    public override void Exit()
    {
        CameraController.Instance.isStopPlayerRotation = false;
        base.Exit();
    }
}
