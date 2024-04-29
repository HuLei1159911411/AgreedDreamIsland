using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestPanel : MonoBehaviour
{
    public PlayerMovementStateMachine playerMovementStateMachine;
    public Text nowView;
    public CameraController mainCamera;
    public Text nowState;
    public Text nowIsOnGround;
    public Text nowXozSpeed;
    public Text nowXoySpeed;
    public Text nowMaxSpeed;
    public Text nowWallState;
    public Text nowHigh;
    
    private E_CameraView _cameraView;
    private string _playerState;
    private bool _playerIsOnGround;
    private bool _playerIsOnSlope;
    private bool _hasWallOnLeft;
    private bool _hasWallOnRight;
    private bool _hasWallOnForward;
    private float _nowHigh;
    // 玩家XOZ平面移动速度大小
    private float _xozSpeed;
    // 玩家XOY平面移动速度大小
    private float _xoySpeed;
    // 玩家当前最大速度
    private float _nowMaxSpeed;
    private void Start()
    {
        _cameraView = mainCamera.nowView;
        UpdateCameraViewText();
        
        _playerState = playerMovementStateMachine.GetNowStateString();
        UpdatePlayerStateText();

        _playerIsOnGround = playerMovementStateMachine.isOnGround;
        _playerIsOnSlope = playerMovementStateMachine.isOnSlope;
        UpdatePlayerIsWhereText();

        _xozSpeed = playerMovementStateMachine.playerXozSpeed;
        _xoySpeed = playerMovementStateMachine.playerXoySpeed;
        UpdatePlayerSpeedText();

        _nowMaxSpeed = playerMovementStateMachine.nowMoveSpeed;
        UpdateNowMaxSpeed();

        _hasWallOnLeft = playerMovementStateMachine.hasWallOnLeft;
        _hasWallOnRight = playerMovementStateMachine.hasWallOnRight;
        _hasWallOnForward = playerMovementStateMachine.hasWallOnForward;
        UpdateNowWallState();

        _nowHigh = playerMovementStateMachine.nowHigh;
        UpdateNowHigh();
    }

    private void FixedUpdate()
    {
        if (_cameraView != mainCamera.nowView)
        {
            _cameraView = mainCamera.nowView;
            UpdateCameraViewText();
        }

        if (_playerState != playerMovementStateMachine.GetNowStateString())
        {
            _playerState = playerMovementStateMachine.GetNowStateString();
            UpdatePlayerStateText();
        }

        if (_playerIsOnGround != playerMovementStateMachine.isOnGround || _playerIsOnSlope != playerMovementStateMachine.isOnSlope)
        {
            _playerIsOnGround = playerMovementStateMachine.isOnGround;
            _playerIsOnSlope = playerMovementStateMachine.isOnSlope;
            UpdatePlayerIsWhereText();
        }

        if (Mathf.Abs(_xozSpeed - playerMovementStateMachine.playerXozSpeed) > float.Epsilon ||
            Mathf.Abs(_xoySpeed - playerMovementStateMachine.playerXoySpeed) > float.Epsilon)
        {
            _xozSpeed = playerMovementStateMachine.playerXozSpeed;
            _xoySpeed = playerMovementStateMachine.playerXoySpeed;
            UpdatePlayerSpeedText();
        }

        if (Math.Abs(_nowMaxSpeed - playerMovementStateMachine.nowMoveSpeed) > float.Epsilon)
        {
            _nowMaxSpeed = playerMovementStateMachine.nowMoveSpeed;
            UpdateNowMaxSpeed();
        }

        if (_hasWallOnLeft != playerMovementStateMachine.hasWallOnLeft ||
            _hasWallOnRight != playerMovementStateMachine.hasWallOnRight ||
            _hasWallOnForward != playerMovementStateMachine.hasWallOnForward)
        {
            _hasWallOnLeft = playerMovementStateMachine.hasWallOnLeft;
            _hasWallOnRight = playerMovementStateMachine.hasWallOnRight;
            _hasWallOnForward = playerMovementStateMachine.hasWallOnForward;
            UpdateNowWallState();
        }

        if (_nowHigh != playerMovementStateMachine.nowHigh)
        {
            _nowHigh = playerMovementStateMachine.nowHigh;
            UpdateNowHigh();
        }
    }

    void UpdateCameraViewText()
    {
        switch (_cameraView)
        {
            case E_CameraView.FirstPerson:
                nowView.text = "第一人称";
                break;
            case E_CameraView.ThirdPerson:
                nowView.text = "第三人称";
                break;
            case E_CameraView.ThirdPersonFurther:
                nowView.text = "更远的第三人称";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void UpdatePlayerStateText()
    {
        switch (_playerState)
        {
            case "Idle":
                nowState.text = "空闲";
                break;
            case "Walk":
                nowState.text = "行走";
                break;
            case "Run":
                nowState.text = "奔跑";
                break;
            case "Jump":
                nowState.text = "跳跃";
                break;
            case "Fall":
                nowState.text = "下落";
                break;
            case "Squat":
                nowState.text = "下蹲";
                break;
            case "Sliding":
                nowState.text = "滑铲";
                break;
            case "WallRunning":
                nowState.text = "滑墙";
                break;
            case "Climb":
                nowState.text = "攀爬";
                break;
            case "Roll":
                nowState.text = "翻滚";
                break;
            case "Grapple":
                nowState.text = "钩锁";
                break;
            case "Null":
                nowState.text = "初始化";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void UpdatePlayerIsWhereText()
    {
        if (_playerIsOnSlope)
        {
            nowIsOnGround.text = "斜面";
        }
        else if (_playerIsOnGround)
        {
            nowIsOnGround.text = "地面";
        }
        else
        {
            nowIsOnGround.text = "空中";
        }
    }

    void UpdatePlayerSpeedText()
    {
        nowXozSpeed.text = "Xoz速度: " + _xozSpeed;
        nowXoySpeed.text = "Xoy速度: " + _xoySpeed;
    }

    void UpdateNowMaxSpeed()
    {
        nowMaxSpeed.text = "当前最大速度: " + _nowMaxSpeed;
    }
    
    void UpdateNowWallState()
    {
        nowWallState.text = $"附近墙壁:{((_hasWallOnLeft ? " 左" : "") + (_hasWallOnRight ? " 右" : "") + (_hasWallOnForward ? " 前" : ""))}";
    }

    void UpdateNowHigh()
    {
        nowHigh.text = "当前高度: " + _nowHigh;
    }
}
