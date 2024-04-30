using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorEvents : MonoBehaviour
{
    public  PlayerMovementStateMachine movementStateMachine;

    public void ChangeStateClimbToFall()
    {
        movementStateMachine.ChangeStateClimbToFall();
    }

    public void ExitRollState()
    {
        movementStateMachine.ExitRollState();
    }

    public void InitGrappleHookTurnParameters()
    {
        movementStateMachine.InitGrappleHookTurnParameters();
    }
}
