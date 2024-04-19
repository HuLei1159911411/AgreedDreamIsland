using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    protected BaseState _currentState;

    void Start()
    {
        _currentState = GetInitialState();
        if (_currentState != null)
        {
            _currentState.Enter();
        }
    }
    protected virtual void Update()
    {
        if (_currentState != null)
        {
            _currentState.UpdateLogic();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_currentState != null)
        {
            _currentState.UpdatePhysic();
        }
    }

    public virtual void ChangeState(BaseState newState)
    {
        _currentState.Exit();
        
        newState.preState = _currentState;
        _currentState = newState;
        
        _currentState.Enter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }
}
