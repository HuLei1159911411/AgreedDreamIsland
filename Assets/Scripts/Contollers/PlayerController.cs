using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public enum E_PlayerStates
{
    Idle,
    Walk,
    Run,
    Jump,
}
public class PlayerController : MonoBehaviour
{
    public float WalkForwardSpeed = 10;
    public float WalkBackwardSpeed = 5;
    public float WalkHorizontal = 5;
    public float RunForwardSpeed = 20;
    public float BehaviorWalkToRunTime = 0.5f;
    public float SpeedWalkToRunTime = 1f;

    private float _nowForawardSpeed;
    private E_PlayerStates _nowState;
    private float _timer;

    void Awake()
    {
        Init();
    }

    void Update()
    {
        ListenPlayerBehaviorInput();
    }

    void Init()
    {
        _nowForawardSpeed = WalkBackwardSpeed;
        _nowState = E_PlayerStates.Idle;
    }

    void ListenPlayerBehaviorInput()
    {
        if (!(InputManager.Instance is null))
        {
            // 摁住前进键并且没摁后退键
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveBackward]))
            {
                if (_nowState == E_PlayerStates.Idle)
                {
                    _nowState = E_PlayerStates.Walk;
                }

                // 移动逻辑
                transform.Translate(Time.deltaTime * _nowForawardSpeed * Vector3.forward);

                // 不在跑步状态并且摁了切换移动模式
                if (_nowState == E_PlayerStates.Walk && Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.ChangeMoveMode]))
                {
                    _timer += Time.deltaTime;
                    if (_timer >= SpeedWalkToRunTime)
                    {
                        _timer = 0;
                        _nowState = E_PlayerStates.Run;
                    }
                }

                // 在切换移动模式的键摁下时间不足时松开切换移动模式的键
                if (_nowState == E_PlayerStates.Walk && _timer < SpeedWalkToRunTime &&
                    Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.ChangeMoveMode]))
                {
                    _timer = 0;
                }
                
                // 在摁下切换移动模式的键并达到规定时间后，改变移动速度到奔跑的移动速度
                if (_nowState == E_PlayerStates.Run && _timer <= SpeedWalkToRunTime)
                {
                    _nowForawardSpeed = Mathf.Lerp(WalkForwardSpeed, RunForwardSpeed, _timer / SpeedWalkToRunTime);

                    _timer += Time.deltaTime;
                }
            }

            // 松开前进键停止前进并改变当前角色的状态为Idle
            if(Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]))
            {
                _nowForawardSpeed = WalkForwardSpeed;
                _nowState = E_PlayerStates.Idle;
                _timer = 0;
            }

            // 摁后退键不改变状态
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveBackward]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]))
            {
                transform.Translate(Time.deltaTime * WalkBackwardSpeed * -Vector3.forward);
            }

            // 摁向左键不改变状态
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]))
            {
                transform.Translate(Time.deltaTime * WalkHorizontal * -Vector3.right);
            }

            // 摁向右键不改变状态
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]))
            {
                transform.Translate(Time.deltaTime * WalkHorizontal * Vector3.right);
            }
        }
    }
}