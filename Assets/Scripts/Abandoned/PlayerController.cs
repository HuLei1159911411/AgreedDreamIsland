using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum E_PlayerStates
{
    Idle,
    Walk,
    Run,
    Jump,
}
public class PlayerController : MonoBehaviour
{
    // 玩家信息参数
    public float PlayerHeight = 1f;
    // 高度检测时玩家距离地面判断高度的浮动值(该值越大在玩家距离地面越远时越有可能判断在地面)
    public float HeightOffset = 0.2f;
    // 地面层
    public LayerMask LayerGround;
    // 玩家运动相关参数
    public float WalkForwardSpeed = 8;
    public float WalkBackwardSpeed = 5;
    public float WalkHorizontal = 5;
    public float RunForwardSpeed = 20;
    // 从Walk到Run所需摁下摁键时间
    public float BehaviorWalkToRunTime = 0.5f;
    public float SpeedWalkToRunTime = 1f;
    // 跳跃力的大小
    public float JumpForce;
    // 当前角色是否在地面上(考虑性能是否要时刻更新该值)
    public bool IsOnGround;
    // 当前玩家状态
    public E_PlayerStates State => _nowState;
    
    // 当前速度
    private float _nowSpeed;
    private E_PlayerStates _nowState;
    private float _timer;
    // 玩家身上的刚体组件
    private Rigidbody _rb;
    // 玩家当前移动方向
    private Vector3 _direction;
    // 当前玩家是否准备切换移动模式
    private bool _isChangeMoveModeWalkToRun;
    // 当前玩家刚体的速度矢量(在计算时起临时变量的作用，不一定是准确的可能只是当时赋值正确)
    private Vector3 _velocity;
    // 检测玩家从行走到奔跑状态切换完成的协程
    private Coroutine _ieWalkToRun;
    // 检测玩家跳跃动作完成的协程
    private Coroutine _ieJumpComplete;
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        Init();
    }

    private void Start()
    {
        CheckWithUpdateIsOnGround();
    }

    void Update()
    {
        CheckWithUpdateIsOnGround();
        ListenPlayerBehaviorInput();
    }

    void Init()
    {
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
                if (_nowState != E_PlayerStates.Walk && _nowState != E_PlayerStates.Run)
                {
                    CheckWithChangeState(E_PlayerStates.Walk);
                }

                // 前进
                Move();

                // 不在跑步状态并且摁了切换移动模式
                if (_nowState == E_PlayerStates.Walk && Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Run]))
                {
                    _ieWalkToRun = StartCoroutine(CompleteBehavior(() =>
                    {
                        _nowState = E_PlayerStates.Run;
                    }));
                }

                // 在切换移动模式的键摁下时间不足时松开切换移动模式的键
                if (_nowState == E_PlayerStates.Walk && _ieWalkToRun != null &&
                    Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.Run]))
                {
                    StopCoroutine(_ieWalkToRun);
                    _ieWalkToRun = null;
                }
                
                // 在摁下切换移动模式的键并达到规定时间后，改变移动速度到奔跑的移动速度
                if (_nowState == E_PlayerStates.Run && _timer <= SpeedWalkToRunTime)
                {
                    _nowSpeed = Mathf.Lerp(WalkForwardSpeed, RunForwardSpeed, _timer / SpeedWalkToRunTime);
                    _timer += Time.deltaTime;
                }
            }

            // 松开前进键停止前进并改变当前角色的状态为Idle
            if(Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]) && _ieJumpComplete == null)
            {
                if (CheckWithChangeState(E_PlayerStates.Idle))
                {
                    if (_ieWalkToRun != null)
                    {
                        StopCoroutine(_ieWalkToRun);
                        _ieWalkToRun = null;
                    } 
                }

            }

            // 摁后退键没摁前进键
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveBackward]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveForward]))
            {
                transform.Translate(Time.deltaTime * WalkBackwardSpeed * -Vector3.forward);
            }

            // 摁向左键没摁右键
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]))
            {
                transform.Translate(Time.deltaTime * WalkHorizontal * -Vector3.right);
            }

            // 摁向右键没摁左键
            if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveRight]) &&
                !Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.MoveLeft]))
            {
                transform.Translate(Time.deltaTime * WalkHorizontal * Vector3.right);
            }
            
            // 摁跳跃键并且在地上
            if (IsOnGround && Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Jump]))
            {
                Jump();
            }
        }
    }
    
    // 检查是否处于地面
    bool CheckWithUpdateIsOnGround()
    {
        IsOnGround = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + HeightOffset, LayerGround);
        return IsOnGround;
    }

    // 移动
    void Move()
    {
        transform.Translate(Time.deltaTime * _nowSpeed * _direction);
    }
    
    // 跳跃
    void Jump()
    {
        _velocity = _rb.velocity;
        // 清空角色垂直方向上的速度
        _rb.velocity = new Vector3(_velocity.x, 0f, _velocity.z);
        // 为角色增加一个向上的力
        _rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
    }

    // 检查并改变人物的状态，当可以修改时修改人物状态，只是修改_nowState的值并不会做出对应行为，当前状态可以转变为目标状态时返回true否则为false
    bool CheckWithChangeState(E_PlayerStates targetState)
    {
        switch (_nowState)
        {
            case E_PlayerStates.Idle:
                switch (targetState)
                {
                    case E_PlayerStates.Idle:
                    case E_PlayerStates.Walk:
                        _nowState = targetState;
                        return true;
                    case E_PlayerStates.Run:
                        return false;
                    case E_PlayerStates.Jump:
                        if (CheckWithUpdateIsOnGround())
                        {
                            _nowState = targetState;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
            case E_PlayerStates.Walk:
                switch (targetState)
                {
                    case E_PlayerStates.Idle:
                    case E_PlayerStates.Walk:
                    case E_PlayerStates.Run:
                        _nowState = targetState;
                        return true;
                    case E_PlayerStates.Jump:
                        if (CheckWithUpdateIsOnGround())
                        {
                            _nowState = targetState;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
            case E_PlayerStates.Run:
                switch (targetState)
                {
                    case E_PlayerStates.Idle:
                    case E_PlayerStates.Walk:
                    case E_PlayerStates.Run:
                        _nowState = targetState;
                        return true;
                    case E_PlayerStates.Jump:
                        if (CheckWithUpdateIsOnGround())
                        {
                            _nowState = targetState;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            
            case E_PlayerStates.Jump:
                switch (targetState)
                {
                    case E_PlayerStates.Idle:
                        if (CheckWithUpdateIsOnGround())
                        {
                            _nowState = targetState;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case E_PlayerStates.Walk:
                    case E_PlayerStates.Run:
                    case E_PlayerStates.Jump:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetState), targetState, null);
                }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 持续检测某个行为是否完成并在完成后调用对应委托
    IEnumerator CompleteBehavior(Action doSomething)
    {
        switch (_nowState)
        {
            case E_PlayerStates.Jump:
                yield return !IsOnGround;
                doSomething.Invoke();
                break;
            case E_PlayerStates.Walk:
                yield return new WaitForSeconds(BehaviorWalkToRunTime);
                doSomething.Invoke();
                break;
        }
    }
}