using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseState
{
    public E_State state;
    public BaseState preState;
    protected StateMachine stateMachine;

    public BaseState(E_State state, StateMachine stateMachine)
    {
        this.state = state;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        
    }
    public virtual void UpdateLogic()
    {
        
    }
    public virtual void UpdatePhysic()
    {
        
    }
    public virtual void Exit()
    {
        
    }
}
