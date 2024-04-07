using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public enum E_CameraView
{
    FirstPerson,
    ThirdPerson,
    ThirdPersonFurther,
}

public class CameraController : MonoBehaviour
{
    // 玩家Transform组件
    public Transform Player;
    // 当前的摄像机视角
    public E_CameraView nowView;
    // 第一人称视角偏移量
    public Vector3 FirstPersonViewOffset = new Vector3(0, 0, 0);
    // 第三人称视角摄像头聚焦点与玩家位置偏移量
    public Vector3 ThridPersonFocusWithPlayerOffset = new Vector3(1, 0, 0);
    // 第三人称视角摄像头位置与玩家边上的聚焦点的距离
    public float Distance = 10f;
    // 更远的第三人称视角距离系数
    public float ThridPersonFurtherRatio = 1.5f;
    // 第三人称视角摄像头法向量与人物面朝向法向量的最大夹角大小
    public float ThridPersonViewCameraWithPlayerMaxAngle = 10;
    // 鼠标移动速度
    public float MouseSpeed = 800f;
    
    // 记录摄像机之前位置玩家边上的聚焦点的坐标
    private Vector3 _focusPosition;
    // 计算当前帧玩家边上的聚焦点的坐标的临时变量
    private Vector3 _curFocusPosition;
    // 旋转后聚焦点到的摄像机单位向量
    private Vector3 _dir;
    // 鼠标X坐标变化
    private float _mouseX;
    // 绕世界坐标up向量旋转角度
    private float _upAngle;
    // 鼠标Y坐标变化
    private float _mouseY;
    // 绕世界坐标right向量旋转角度
    private float _rightAngle;
    // 玩家
    RaycastHit hit;

    void Start()
    {
        Init();
    }

    void Update()
    {
    }

    void LateUpdate()
    {
        ListenCameraBehaviorInput();
    }

    public void ChangeCameraView(E_CameraView view)
    {
        nowView = view;
        UpdateFocusPosition();
        UpdateCameraPositionWithRotation();
    }

    void Init()
    {
        nowView = E_CameraView.ThirdPerson;
        LockWithHideCursor();
    }

    // 监听与摄像机有关行为输入
    void ListenCameraBehaviorInput()
    {
        if (!(InputManager.Instance is null))
        {
            // 切换摄像机视角(同时隐藏鼠标)
            if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.ChangeView]))
            {
                if (nowView != E_CameraView.ThirdPersonFurther)
                {
                    nowView++;
                }
                else
                {
                    nowView = 0;
                }

                LockWithHideCursor();
            }

            // 监听鼠标输入
            _mouseX = Input.GetAxis("Mouse X");
            _mouseY = Input.GetAxis("Mouse Y");
            
            if (_mouseX != 0 || _mouseY != 0 || UpdateFocusPosition())
            {
                // 如果是移动鼠标
                if (_mouseX != 0 || _mouseY != 0)
                {
                    // 更新聚焦点的位置
                    UpdateFocusPosition();
                }
                // 对鼠标输入进行角度变化的转换
                // 上下角度变化
                _rightAngle -= _mouseY * MouseSpeed * Time.deltaTime;
                // 左右角度变化
                _upAngle += _mouseX * MouseSpeed * Time.deltaTime;
                switch (nowView)
                {
                    // 第一人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.FirstPerson:
                        // 第一人称摄像机视角旋转，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度
                        _rightAngle = Mathf.Clamp(_rightAngle, -90f, 90f);
                        // 更新摄像机位置和旋转
                        UpdateCameraPositionWithRotation();
                        break;
                    // 第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.ThirdPerson:
                        // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                        _rightAngle = ClampAngle(_rightAngle, -89.99f, 89.99f);
                        // 更新摄像机位置和旋转
                        UpdateCameraPositionWithRotation();
                        break;
                    // 更远的第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.ThirdPersonFurther:
                        // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                        _rightAngle = ClampAngle(_rightAngle, -89.99f, 89.99f);
                        // 更新摄像机位置和旋转
                        UpdateCameraPositionWithRotation();
                        break;
                }
            }
        }
    }

    void LockWithHideCursor()
    {
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    bool UpdateFocusPosition()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                _curFocusPosition = Player.position +
                                    FirstPersonViewOffset.x * Player.right +
                                    FirstPersonViewOffset.y * Player.up +
                                    FirstPersonViewOffset.z * Player.forward;
                if (_focusPosition == _curFocusPosition)
                {
                    return false;
                }
                else
                {
                    _focusPosition = _curFocusPosition;
                    return true;
                }
            case E_CameraView.ThirdPerson:
            case E_CameraView.ThirdPersonFurther:
                _curFocusPosition = Player.position +
                                    ThridPersonFocusWithPlayerOffset.x * Player.right +
                                    ThridPersonFocusWithPlayerOffset.y * Player.up +
                                    ThridPersonFocusWithPlayerOffset.z * Player.forward;
                if (_focusPosition == _curFocusPosition)
                {
                    return false;
                }
                else
                {
                    _focusPosition = _curFocusPosition;
                    return true;
                }
        }

        throw new IndexOutOfRangeException("CameraController.nowView is not exist");
    }
    
    // 限制上下旋转的角度
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    
    // 检测Player到摄像机之间是否有遮挡有则摄像机到玩家距离缩短到射线碰撞的距离
    Vector3 CheckPlayerToTargetPoint(Vector3 position)
    {
        if (Physics.Linecast(Player.position, position, out hit))
        {
            if (!hit.collider.gameObject.CompareTag("MainCamera") && !hit.collider.gameObject.CompareTag("Player"))
            {
                // 如果射线碰撞的不是相机，返回修正后的相机位置
                return hit.point;
            }
        }
        
        return position;
    }
    
    // 更新摄像机位置及旋转
    public void UpdateCameraPositionWithRotation()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                // 第一人称摄像机位置旋转更新
                // 更新摄像机位置
                transform.position = _focusPosition;
                // 旋转摄像机
                transform.rotation = Quaternion.Euler(_rightAngle, _upAngle, 0);
                // 旋转角色，根据鼠标左右移动旋转
                Player.rotation = Quaternion.Euler(0, _upAngle, 0);
                break;
            case E_CameraView.ThirdPerson:
                // 第三人称摄像机位置旋转更新
                // 计算摄像头绕聚焦点上下旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_rightAngle, Vector3.right) * Vector3.forward;
                // 计算摄像头绕聚焦点左右旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_upAngle, Vector3.up) * _dir;
                // 设置左右旋转后摄像机的位置
                transform.position = CheckPlayerToTargetPoint(_focusPosition + Distance * -_dir);
                // 旋转摄像机
                transform.rotation = Quaternion.LookRotation(_dir);
                break;
            case E_CameraView.ThirdPersonFurther:
                // 更远的第三人称摄像机位置旋转更新
                // 计算摄像头绕聚焦点上下旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_rightAngle, Vector3.right) * Vector3.forward;
                // 计算摄像头绕聚焦点左右旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_upAngle, Vector3.up) * _dir;
                // 设置左右旋转后摄像机的位置
                transform.position =
                    CheckPlayerToTargetPoint(_focusPosition + Distance * ThridPersonFurtherRatio * -_dir);
                // 旋转摄像机
                transform.rotation = Quaternion.LookRotation(_dir);
                break;
        }
    }
}