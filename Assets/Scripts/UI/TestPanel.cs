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
    public Text nowSpeed;
    public Text nowMaxSpeed;
    
    private E_CameraView _cameraView;
    private BaseState _playerState;
    private bool _playerIsOnGround;
    private bool _playerIsOnSlope;
    // 玩家XOZ平面移动速度大小
    private float _playerSpeed;
    // 玩家当前最大速度
    private float _nowMaxSpeed;
    private void Start()
    {
        _cameraView = mainCamera.nowView;
        UpdateCameraViewText();
        
        _playerState = playerMovementStateMachine.CurrentState;
        UpdatePlayerStateText();

        _playerIsOnGround = playerMovementStateMachine.isOnGround;
        _playerIsOnSlope = playerMovementStateMachine.isOnSlope;
        UpdatePlayerIsWhereText();

        _playerSpeed = playerMovementStateMachine.playerXozSpeed;
        UpdatePlayerSpeedText();

        _nowMaxSpeed = playerMovementStateMachine.nowMoveSpeed;
        UpdateNowMaxSpeed();
    }

    void Update()
    {
        if (_cameraView != mainCamera.nowView)
        {
            _cameraView = mainCamera.nowView;
            UpdateCameraViewText();
        }

        if (_playerState != playerMovementStateMachine.CurrentState)
        {
            _playerState = playerMovementStateMachine.CurrentState;
            UpdatePlayerStateText();
        }

        if (_playerIsOnGround != playerMovementStateMachine.isOnGround || _playerIsOnSlope != playerMovementStateMachine.isOnSlope)
        {
            _playerIsOnGround = playerMovementStateMachine.isOnGround;
            _playerIsOnSlope = playerMovementStateMachine.isOnSlope;
            UpdatePlayerIsWhereText();
        }

        if (Math.Abs(_playerSpeed - playerMovementStateMachine.playerXozSpeed) > Single.Epsilon)
        {
            _playerSpeed = playerMovementStateMachine.playerXozSpeed;
            UpdatePlayerSpeedText();
        }

        if (Math.Abs(_nowMaxSpeed - playerMovementStateMachine.nowMoveSpeed) > Single.Epsilon)
        {
            _nowMaxSpeed = playerMovementStateMachine.nowMoveSpeed;
            UpdateNowMaxSpeed();
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
        switch (_playerState.name)
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
        nowSpeed.text = "速度: " + _playerSpeed.ToString("F1");
    }

    void UpdateNowMaxSpeed()
    {
        nowMaxSpeed.text = "当前最大速度: " + _nowMaxSpeed;
    }
}
