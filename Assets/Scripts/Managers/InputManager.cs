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
    Roll,
    HookShootLeft,
    HookShootRight,
    Fire,
    Aim,
    Interact,
    FirstWeapon,
    SecondWeapon,
    CancelWeapon,
    OpenBagPanel,
    OpenTestPanel,
    LockViewToFollowMonster,
}
public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance => _instance;

    public Dictionary<E_InputBehavior, KeyCode> DicBehavior;

    private void Awake()
    {
        if (_instance == null)
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
            { E_InputBehavior.Roll, KeyCode.LeftShift },
            { E_InputBehavior.HookShootLeft, KeyCode.Q },
            { E_InputBehavior.HookShootRight, KeyCode.E },
            { E_InputBehavior.Fire, KeyCode.Mouse0 },
            { E_InputBehavior.Aim, KeyCode.Mouse1 },
            { E_InputBehavior.Interact, KeyCode.F },
            { E_InputBehavior.FirstWeapon, KeyCode.Alpha1 },
            { E_InputBehavior.SecondWeapon, KeyCode.Alpha2 },
            { E_InputBehavior.CancelWeapon, KeyCode.Alpha3 },
            { E_InputBehavior.OpenBagPanel, KeyCode.Tab },
            { E_InputBehavior.OpenTestPanel, KeyCode.Equals },
            { E_InputBehavior.LockViewToFollowMonster, KeyCode.BackQuote },
        };
    }
}
