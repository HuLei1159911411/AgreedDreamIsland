using System;
using UnityEngine;

public enum E_CameraView
{
    FirstPerson,
    ThirdPerson,
    ThirdPersonFurther,
}

public class CameraController : MonoBehaviour
{
    // 玩家Transform组件
    public Transform player;
    // 当前的摄像机视角
    public E_CameraView nowView;
    // 第一人称视角偏移量
    public Vector3 firstPersonViewOffset = new Vector3(0, 0, 0);
    // 第三人称视角摄像头聚焦点与玩家位置偏移量
    public Vector3 thirdPersonFocusWithPlayerOffset = new Vector3(1, 0, 0);
    // 第三人称视角摄像头位置与玩家边上的聚焦点的距离
    public float distance = 10f;
    // 更远的第三人称视角距离系数
    public float thirdPersonFurtherRatio = 1.5f;
    // 第三人称视角摄像头法向量与人物面朝向法向量的最大夹角大小
    public float thirdPersonViewCameraWithPlayerMaxAngle = 10;
    // 鼠标移动速度
    public float mouseSpeed = 800f;
    // 第三人称摄像机与人物夹角过大时减少夹角至0所需的时间
    public float rotateTime = 0.1f;
    
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
    // Player向摄像机发射的射线的击中信息
    RaycastHit _hit;
    // 玩家在Xoz平面的面朝向向量
    private Vector3 _playerForwardXoz;
    // 摄像机在Xoz平面的面朝向向量
    private Vector3 _cameraForwardXoz;
    // 玩家面朝向与摄像机面朝向的夹角
    private float _xozAngle;
    // 玩家目标绕Y轴旋转四元数
    private Quaternion _targetRotation;
    // 玩家之前绕Y轴旋转四元数
    private Quaternion _startRotation;
    // 人物旋转至目标角度计时器
    private float _rotateTimer;

    // 组件
    private PlayerMovementStateMachine _playerMovementStateMachine;

    private void Awake()
    {
        _playerMovementStateMachine = player.GetComponent<PlayerMovementStateMachine>();
    }

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
        LockWithHideCursor();
        InitParameters();
    }
    
    // 初始化参数
    private void InitParameters()
    {
        nowView = E_CameraView.ThirdPerson;
        _startRotation = Quaternion.identity;
        _targetRotation = Quaternion.identity;
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
                    ChangeCameraView(nowView + 1);
                }
                else
                {
                    ChangeCameraView(0);
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
                _rightAngle -= _mouseY * mouseSpeed * Time.deltaTime;
                // 左右角度变化
                _upAngle += _mouseX * mouseSpeed * Time.deltaTime;
                switch (nowView)
                {
                    // 第一人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.FirstPerson:
                        // 第一人称摄像机视角旋转，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度
                        _rightAngle = Mathf.Clamp(_rightAngle, -90f, 90f);
                        // 更新摄像机位置及旋转
                        UpdateCameraPositionWithRotation();
                        break;
                    // 第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.ThirdPerson:
                        // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                        _rightAngle = ClampAngle(_rightAngle, -89.99f, 89.99f);
                        // 更新摄像机位置及旋转
                        UpdateCameraPositionWithRotation();
                        break;
                    // 更远的第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                    case E_CameraView.ThirdPersonFurther:
                        // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                        // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                        _rightAngle = ClampAngle(_rightAngle, -89.99f, 89.99f);
                        // 更新摄像机位置及旋转
                        UpdateCameraPositionWithRotation();
                        break;
                }
            }
            // 一直都要更新，因为有可能在Idle状态下玩家的角度和摄像机角度差距过大，但是玩家的角度还没同步到目标角度 但是玩家并没进行任何操作
            else if (!(_playerMovementStateMachine is null) && _playerMovementStateMachine.GetNowState() == "Idle")
            {
                if (player.rotation != _targetRotation)
                {
                    StartToTargetRotation();
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

    // 更新聚焦点位置
    bool UpdateFocusPosition()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                _curFocusPosition = player.position +
                                    firstPersonViewOffset.x * player.right +
                                    firstPersonViewOffset.y * player.up +
                                    firstPersonViewOffset.z * player.forward;
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
                _curFocusPosition = player.position +
                                    thirdPersonFocusWithPlayerOffset.x * player.right +
                                    thirdPersonFocusWithPlayerOffset.y * player.up +
                                    thirdPersonFocusWithPlayerOffset.z * player.forward;
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
    
    // 检测聚焦点到摄像机之间是否有环境层的遮挡，有则摄像机到聚焦点距离缩短到射线碰撞的距离
    Vector3 CheckFocusToTargetPoint(Vector3 position)
    {
        Debug.DrawLine(_focusPosition, position, Color.green);
        // 在编辑器模式下预览相机位置也会进行这个射线检测，所以需要对InfoManager进行判空处理，因为InfoManager是继承MonoBehaviour的单例，在执行Awake时才会去进行赋值，所以在编辑器编辑器模式下InfoManager为空
        if (!(InfoManager.Instance is null) && Physics.Linecast(_focusPosition, position, out _hit, InfoManager.Instance.layerGround))
        {
            if (!_hit.collider.gameObject.CompareTag("MainCamera") )
            {
                // 如果射线碰撞的不是相机，返回修正后的相机位置
                return _hit.point;
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
                
                // 更新玩家旋转
                UpdatePlayerTargetAngle();
                break;
            case E_CameraView.ThirdPerson:
                // 第三人称摄像机位置旋转更新
                // 计算摄像头绕聚焦点上下旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_rightAngle, Vector3.right) * Vector3.forward;
                // 计算摄像头绕聚焦点左右旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_upAngle, Vector3.up) * _dir;
                // 设置左右旋转后摄像机的位置
                transform.position = CheckFocusToTargetPoint(_focusPosition + distance * -_dir);
                // 旋转摄像机
                transform.rotation = Quaternion.LookRotation(_dir);
                
                // 更新玩家旋转
                UpdatePlayerTargetAngle();
                break;
            case E_CameraView.ThirdPersonFurther:
                // 更远的第三人称摄像机位置旋转更新
                // 计算摄像头绕聚焦点上下旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_rightAngle, Vector3.right) * Vector3.forward;
                // 计算摄像头绕聚焦点左右旋转后聚焦点到摄像机的方向向量
                _dir = Quaternion.AngleAxis(_upAngle, Vector3.up) * _dir;
                // 设置左右旋转后摄像机的位置
                transform.position =
                    CheckFocusToTargetPoint(_focusPosition + distance * thirdPersonFurtherRatio * -_dir);
                // 旋转摄像机
                transform.rotation = Quaternion.LookRotation(_dir);
                
                // 更新玩家旋转
                UpdatePlayerTargetAngle();
                break;
        }
    }
    
    // 更新玩家旋转目标角度(第一人称直接跟随摄像机旋转，第三人称有过程旋转)
    void UpdatePlayerTargetAngle()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                // 旋转角色，根据鼠标左右移动旋转
                player.rotation = Quaternion.Euler(0, _upAngle, 0);
                _targetRotation = player.rotation;
                break;
            case E_CameraView.ThirdPerson:
                // 让玩家移动状态机为不为空时(及非编辑器模式下)||Idle状态下当前摄像机朝向和人物朝向的两向量夹角大于某一角度时人物旋转至摄像机朝向
                if (!(_playerMovementStateMachine is null) && _playerMovementStateMachine.GetNowState() == "Idle")
                {
                    if (CheckPlayerWithCameraAngleInXoz())
                    {
                        // 通过一定时间旋转成摄像机的角度
                        _startRotation = player.rotation.normalized;
                        _targetRotation = (player.rotation * Quaternion.Euler(0, _upAngle - player.rotation.eulerAngles.y, 0)).normalized;
                        _rotateTimer = 0f;
                        StartToTargetRotation();
                    }
                }
                // 其他状态保持当前摄像机朝向与人物朝向一致，并且将玩家的旋转目标四元数设置为当前玩家旋转四元数，以防止玩家在结束当前状态回到Idle状态后继续旋转发生抽搐
                else
                {
                    // 直接将玩家的绕y轴旋转的欧拉角信息同步成摄像机的旋转信息
                    player.rotation *= Quaternion.Euler(0, _upAngle - player.rotation.eulerAngles.y, 0);
                    
                    // 将目标四元数设置为当前玩家旋转四元数
                    _targetRotation = player.rotation;
                }
                break;
            case E_CameraView.ThirdPersonFurther:
                // 让玩家移动状态机为不为空时(及非编辑器模式下)||Idle状态下当前摄像机朝向和人物朝向的两向量夹角大于某一角度时人物旋转至摄像机朝向
                if (!(_playerMovementStateMachine is null) && _playerMovementStateMachine.GetNowState() == "Idle")
                {
                    if (CheckPlayerWithCameraAngleInXoz())
                    {
                        // 通过一定时间旋转成摄像机的角度
                        _startRotation = player.rotation.normalized;
                        _targetRotation = (player.rotation * Quaternion.Euler(0, _upAngle - player.rotation.eulerAngles.y, 0)).normalized;
                        _rotateTimer = 0f;
                        StartToTargetRotation();
                    }
                    // 其他状态保持当前摄像机朝向与人物朝向一致，并且将玩家的旋转目标四元数设置为当前玩家旋转四元数，以防止玩家在结束当前状态回到Idle状态后继续旋转发生抽搐
                    else
                    {
                        // 直接将玩家的绕y轴旋转的欧拉角信息同步成摄像机的旋转信息
                        player.rotation *= Quaternion.Euler(0, _upAngle - player.rotation.eulerAngles.y, 0);
                        
                        // 将目标四元数设置为当前玩家旋转四元数
                        _targetRotation = player.rotation;
                    }
                }
                // 当前玩家旋转不是目标旋转时产生旋转(下面代码限制_rotateTimer / rotateTime为0~0.99是因为会报错,报错是因为四元数插值出现问题，上述代码可能在两端都有一个（0，0，0，0）四元数)
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 检查人物Forward向量在Xoz平面与摄像机Forward向量的夹角是否小于最大夹角
    bool CheckPlayerWithCameraAngleInXoz()
    {
        _playerForwardXoz = new Vector3(player.forward.x, 0, player.forward.z);
        _cameraForwardXoz = new Vector3(transform.forward.x, 0, transform.forward.z);
        _xozAngle = Vector3.Angle(_playerForwardXoz, _cameraForwardXoz);
        return _xozAngle >= thirdPersonViewCameraWithPlayerMaxAngle || _xozAngle <= -thirdPersonViewCameraWithPlayerMaxAngle;
    }
    
    // 玩家四元数向目标四元数旋转
    void StartToTargetRotation()
    {
        if (_rotateTimer < rotateTime)
        {
            _rotateTimer += Time.deltaTime;
            player.rotation = Quaternion.Lerp(_startRotation, _targetRotation, _rotateTimer / rotateTime);
        }
    }
}