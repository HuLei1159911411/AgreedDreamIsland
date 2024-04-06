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
    ChangeMoveMode,
    ChangeView,
}
public class InputManager
{
    private static InputManager _instance = new InputManager();

    public static InputManager Instance => _instance;

    public Dictionary<E_InputBehavior, KeyCode> DicBehavior = new Dictionary<E_InputBehavior, KeyCode>();

    private InputManager()
    {
        InitDicBehavior();
    }
    
    void InitDicBehavior()
    {
        DicBehavior.Add(E_InputBehavior.MoveForward, KeyCode.W);
        DicBehavior.Add(E_InputBehavior.MoveBackward, KeyCode.S);
        DicBehavior.Add(E_InputBehavior.MoveLeft, KeyCode.A);
        DicBehavior.Add(E_InputBehavior.MoveRight, KeyCode.D);
        DicBehavior.Add(E_InputBehavior.Jump, KeyCode.Space);
        DicBehavior.Add(E_InputBehavior.ChangeMoveMode, KeyCode.LeftShift);
        DicBehavior.Add(E_InputBehavior.ChangeView, KeyCode.V);
    }
}
