using System;using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum E_InputBehavior
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    Jump,
    Run,
    ChangeView,
    Squat,
    Sliding,
    HookShootLeft,
    HookShootRight,
    Fire,
    Aim,
}
public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance => _instance;

    public Dictionary<E_InputBehavior, KeyCode> DicBehavior;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        
        InitDicBehavior();
    }
    
    private InputManager()
    {
        InitDicBehavior();
    }
    
    void InitDicBehavior()
    {
        DicBehavior = new Dictionary<E_InputBehavior, KeyCode>()
        {
            { E_InputBehavior.MoveForward, KeyCode.W },
            { E_InputBehavior.MoveBackward, KeyCode.S },
            { E_InputBehavior.MoveLeft, KeyCode.A },
            { E_InputBehavior.MoveRight, KeyCode.D },
            { E_InputBehavior.Jump, KeyCode.Space },
            { E_InputBehavior.Run, KeyCode.LeftShift },
            { E_InputBehavior.ChangeView, KeyCode.V },
            { E_InputBehavior.Squat, KeyCode.LeftControl },
            { E_InputBehavior.Sliding, KeyCode.LeftControl },
            { E_InputBehavior.HookShootLeft, KeyCode.Q },
            { E_InputBehavior.HookShootRight, KeyCode.E },
            { E_InputBehavior.Fire, KeyCode.Mouse0 },
            { E_InputBehavior.Aim , KeyCode.Mouse1 },
        };
    }
}
