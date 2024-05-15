using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum E_CameraView
{
    FirstPerson,
    ThirdPerson,
    ThirdPersonFurther,
}

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance => _instance;
        
    // 玩家Transform
    public Transform playerTransform;
    // 当前的摄像机视角
    public E_CameraView nowView;
    // 当前视角数量
    private int _nowViewCount;
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
    // 第三人称摄像机与人物夹角过大时减少夹角至0时人物旋转的速度
    public float playerRotateSpeed = 0.1f;
    // 第三人称摄像机跟踪速度
    public float cameraFollowSpeed = 0.05f;
    // 第三人称摄像机跟踪惰性距离
    public float cameraInertDistance = 0.1f;
    // 第三人称摄像机旋转速度
    public float cameraRotateSpeed = 0.1f;
    // 是否冻结摄像机旋转和移动
    public bool isStopCameraPositionAndRotation;
    // 是否冻结玩家旋转
    public bool isStopPlayerRotation;
    // 当前人物是否在旋转至目标角度
    public bool playerIsRotating;
    // 当前摄像机是否在移动至目标点并且旋转至目标角度
    public bool cameraIsMovingAndRotating;
    // 锁定怪物最大距离
    public float followMonsterDistance;
    // 是否正在追踪怪物
    public bool isFollowMonster;
    // 追踪的怪物的monsterStateMachine组件
    public MonsterStateMachine nowFollowMonster;
    // 追踪怪物时追踪点在怪物身上的偏移
    public Vector3 followMonsterTargetOffset;
    // 追踪怪物时摄像机偏移
    public Vector3 followMonsterThirdPersonViewCameraOffset;
    
    // 摄像机跟踪目标Transform组件
    private Transform _targetTransform;
    // 记录摄像机之前位置玩家边上的聚焦点的坐标
    private Vector3 _focusPosition;
    // 计算当前帧玩家边上的聚焦点的坐标的临时变量
    private Vector3 _curFocusPosition;
    // 旋转后聚焦点到的摄像机单位向量
    private Vector3 _dir;
    // 旋转后聚焦点到摄像机方向向量在XOZ平面的投影
    private Vector3 _dirXoz;
    // 鼠标X坐标变化
    private float _mouseX;
    // 绕世界坐标up向量旋转角度
    public float upAngle;
    // 鼠标Y坐标变化
    private float _mouseY;
    // 绕世界坐标right向量旋转角度
    public float rightAngle;
    // Focus向摄像机发射的射线的击中信息
    private RaycastHit _focusToCameraHit;
    // 玩家在Xoz平面的面朝向向量
    private Vector3 _playerForwardXoz;
    // 摄像机在Xoz平面的面朝向向量
    private Vector3 _cameraForwardXoz;
    // 玩家面朝向与摄像机面朝向的夹角
    private float _xozAngle;
    
    #region Coroutine
    // 控制人物旋转至目标角度协程
    private Coroutine _playerRotateCoroutine;
    // 玩家目标绕Y轴旋转四元数
    private Quaternion _playerTargetRotation;
    // 用来临时保存计算每帧目标移动位置的变量
    private Vector3 _calculateTargetPosition;
    // 摄像机移动旋转协程
    private Coroutine _cameraMoveAndRotateCoroutine;
    // 摄像机目标移动位置
    private Vector3 _targetPosition;
    // 摄像机目标旋转
    private Quaternion _targetRotation;
    // 协程yield return变量
    private WaitForFixedUpdate _waitForFixedUpdate;
    
    // 摄像机聚焦点默认偏移
    [HideInInspector] public Vector3 focusDefaultOffset;
    // 摄像机目标偏移
    [HideInInspector] public Vector3 focusTargetOffset;
    // 摄像机反方向偏移
    [HideInInspector] public Vector3 reverseFocusOffset;
    // 是否进行偏移
    [HideInInspector] public bool isMoveCameraFocusOffset;
    // 对聚焦点进行移动偏移的协程
    private Coroutine _focusMoveCoroutine;
    
    // 摄像机第三人称下坐标与聚焦点的默认偏移量
    private Vector3 _thirdPersonFocusWithPlayerDefaultOffset;
    // 摄像机第三人称下坐标与聚焦点的变化目标偏移量
    private Vector3 _thirdPersonFocusWithPlayerTargetOffset;
    // 摄像机第三人称下坐标与聚焦点偏移量是否在进行变化
    private bool _isChangeThirdPersonFocusWithPlayerOffset;
    // 摄像机第三人称下坐标与聚焦点偏移量变化协程
    private Coroutine _changeThirdPersonFocusWithPlayerOffsetCoroutine;
    
    #endregion
    
    [Range(0f,1f)]
    // 聚焦点移动的速度
    public float focusMoveRate;

    [Range(0f, 1f)]
    // 第三人称摄像机与聚焦点的偏移量的变化速度
    public float thirdPersonFocusWithPlayerOffsetChangeRate;
    
    // 不同的状态下第三人称摄像机偏移量与默认状态下的值的差值列表(下标为对应状态枚举),X越大越偏右,Y越小越偏上,Z越大越远
    public List<Vector3> listThirdPersonViewFocusOffsetChangeOffset;
    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        AwakeInit();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        ListenCameraBehaviorInput();
    }

    public void ChangeCameraView(E_CameraView view)
    {
        nowView = view;
        UpdateFocusPosition();
        UpdateCameraPositionWithRotation();
    }

    private void AwakeInit()
    {
        _waitForFixedUpdate = new WaitForFixedUpdate();
        _thirdPersonFocusWithPlayerDefaultOffset = thirdPersonFocusWithPlayerOffset;

        _nowViewCount = Enum.GetValues(typeof(E_CameraView)).Length;
    }
    
    private void Init()
    {
        InitParameters();
        
        LockWithHideCursor();
        
        UpdateCameraPositionAndRotationImmediately();

        if (_cameraMoveAndRotateCoroutine != null)
        {
            StopCoroutine(_cameraMoveAndRotateCoroutine);
        }

        if (_playerRotateCoroutine != null)
        {
            StopCoroutine(_playerRotateCoroutine);
        }
        
        _cameraMoveAndRotateCoroutine = StartCoroutine(CameraMoveAndRotateToTarget());
        _playerRotateCoroutine = StartCoroutine(PlayerRotateToTargetQuaternion());
    }
    
    // 初始化参数
    private void InitParameters()
    {
        playerTransform = PlayerMovementStateMachine.Instance.transform;
        
        nowView = E_CameraView.ThirdPerson;
        _targetTransform = playerTransform;

        isStopCameraPositionAndRotation = false;
        isStopPlayerRotation = false;
        
        _playerTargetRotation = playerTransform.rotation;
        _targetPosition = transform.position;
        _targetRotation = transform.rotation;
        
        focusDefaultOffset = thirdPersonFocusWithPlayerOffset;
        reverseFocusOffset = new Vector3(-focusDefaultOffset.x, focusDefaultOffset.y, focusDefaultOffset.z);

        isFollowMonster = false;
        
        upAngle = 0f;
        rightAngle = -4.5f;
    }

    // 监听与摄像机有关行为输入
    private void ListenCameraBehaviorInput()
    {
        if (!(InputManager.Instance is null))
        {
            // 切换摄像机视角(同时隐藏鼠标)
            if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.ChangeView]))
            {
                ChangeCameraView((E_CameraView)(((int)nowView + 1) % _nowViewCount));

                LockWithHideCursor();
            }
            
            if (!isFollowMonster)
            {
                // 视野锁定最近的怪物
                if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.LockViewToFollowMonster]) &&
                    PlayerMovementStateMachine.Instance.CurrentState.state != E_State.Death &&
                    GameManager.Instance.canAccessListMonsters &&
                    GameManager.Instance.listMonsters.Count > 0 &&
                    GameManager.Instance.GetNearestMonster() <= followMonsterDistance)
                {
                    ChangeCameraToFollowMonster();
                    
                    LockWithHideCursor();

                    return;
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
                    rightAngle -= _mouseY * mouseSpeed * Time.deltaTime;
                    // 左右角度变化
                    upAngle += _mouseX * mouseSpeed * Time.deltaTime;
                    if (upAngle > 360f)
                    {
                        upAngle -= 360f;
                    }

                    if (upAngle < -360f)
                    {
                        upAngle += 360f;
                    }

                    switch (nowView)
                    {
                        // 第一人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                        case E_CameraView.FirstPerson:
                            // 第一人称摄像机视角旋转，对鼠标输入进行角度变化处理
                            // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度
                            rightAngle = Mathf.Clamp(rightAngle, -90f, 90f);
                            // 更新摄像机位置及旋转
                            UpdateCameraPositionWithRotation();
                            break;
                        // 第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                        case E_CameraView.ThirdPerson:
                            // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                            // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                            rightAngle = ClampAngle(rightAngle, -89.99f, 89.99f);
                            // 更新摄像机位置及旋转
                            UpdateCameraPositionWithRotation();
                            break;
                        // 更远的第三人称摄像机逻辑的更新摄像机位置并根据鼠标X和Y变换旋转角度
                        case E_CameraView.ThirdPersonFurther:
                            // 第三人称摄像机视角旋转(根据鼠标移动的变化产生的变化与第一人称一样)，对鼠标输入进行角度变化处理
                            // 限制绕right旋转最大角度为从上往下看的角度和从下往上看的角度(89.99和-89.99防止万象锁)
                            rightAngle = ClampAngle(rightAngle, -89.99f, 89.99f);
                            // 更新摄像机位置及旋转
                            UpdateCameraPositionWithRotation();
                            break;
                    }
                }
            }
            else
            {
                // 取消锁定
                if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.LockViewToFollowMonster]))
                {
                    ResetCameraToFollowPlayer();
                    
                    LockWithHideCursor();
                }

                switch (nowView)
                {
                    case E_CameraView.FirstPerson:
                        _calculateTargetPosition = nowFollowMonster.transform.position +
                                                   nowFollowMonster.transform.right * followMonsterTargetOffset.x +
                                                   nowFollowMonster.transform.up * followMonsterTargetOffset.y +
                                                   nowFollowMonster.transform.forward * followMonsterTargetOffset.z;
                        transform.position = playerTransform.position + firstPersonViewOffset;
                        transform.rotation = Quaternion.LookRotation(_calculateTargetPosition - transform.position)
                            .normalized;
                        playerTransform.rotation = Quaternion.LookRotation(-nowFollowMonster.monsterToPlayerDirection);
                        break;
                    case E_CameraView.ThirdPerson:
                        _calculateTargetPosition = nowFollowMonster.transform.position +
                                                   nowFollowMonster.transform.right * followMonsterTargetOffset.x +
                                                   nowFollowMonster.transform.up * followMonsterTargetOffset.y +
                                                   nowFollowMonster.transform.forward * followMonsterTargetOffset.z;
                        _dir = playerTransform.position;
                        _dir += playerTransform.right * followMonsterThirdPersonViewCameraOffset.x +
                                playerTransform.up * followMonsterThirdPersonViewCameraOffset.y +
                                playerTransform.forward * followMonsterThirdPersonViewCameraOffset.z;
                        _targetPosition = _dir;
                        _targetRotation =
                            Quaternion.LookRotation(_calculateTargetPosition - transform.position);
                        playerTransform.rotation = Quaternion.LookRotation(-nowFollowMonster.monsterToPlayerDirection);
                        break;
                    case E_CameraView.ThirdPersonFurther:
                        _calculateTargetPosition = nowFollowMonster.transform.position +
                                                   nowFollowMonster.transform.right * followMonsterTargetOffset.x +
                                                   nowFollowMonster.transform.up * followMonsterTargetOffset.y +
                                                   nowFollowMonster.transform.forward * followMonsterTargetOffset.z;
                        _dir = playerTransform.position;
                        _dir += playerTransform.right * (followMonsterThirdPersonViewCameraOffset.x * thirdPersonFurtherRatio) +
                                playerTransform.up * (followMonsterThirdPersonViewCameraOffset.y * thirdPersonFurtherRatio) +
                                playerTransform.forward * (followMonsterThirdPersonViewCameraOffset.z * thirdPersonFurtherRatio);
                        _targetPosition = _dir;
                        _targetRotation =
                            Quaternion.LookRotation(_calculateTargetPosition - transform.position);
                        playerTransform.rotation = Quaternion.LookRotation(-nowFollowMonster.monsterToPlayerDirection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void LockWithHideCursor()
    {
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 更新聚焦点位置
    private bool UpdateFocusPosition()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                _curFocusPosition = _targetTransform.position +
                                    firstPersonViewOffset.x * _targetTransform.right +
                                    firstPersonViewOffset.y * _targetTransform.up +
                                    firstPersonViewOffset.z * _targetTransform.forward;
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
                _curFocusPosition = _targetTransform.position +
                                    thirdPersonFocusWithPlayerOffset.x * _targetTransform.right +
                                    thirdPersonFocusWithPlayerOffset.y * _targetTransform.up +
                                    thirdPersonFocusWithPlayerOffset.z * _targetTransform.forward;
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
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }

        if (angle > 360)
        {
            angle -= 360;
        }
            
        return Mathf.Clamp(angle, min, max);
    }
    
    // 检测聚焦点到摄像机之间是否有环境层的遮挡，有则摄像机到聚焦点距离缩短到射线碰撞的距离
    private Vector3 CheckFocusToTargetPoint(Vector3 position)
    {
        Debug.DrawLine(_focusPosition, position, Color.green);
        // 在编辑器模式下预览相机位置也会进行这个射线检测，所以需要对InfoManager进行判空处理，因为InfoManager是继承MonoBehaviour的单例，在执行Awake时才会去进行赋值，所以在编辑器编辑器模式下InfoManager为空
        if (!(InfoManager.Instance is null) && Physics.Linecast(_focusPosition, position,
                out _focusToCameraHit, InfoManager.Instance.layerCameraCheck))
        {
            if (!_focusToCameraHit.collider.gameObject.CompareTag("MainCamera") )
            {
                // 如果射线碰撞的不是相机，返回修正后的相机位置
                return _focusToCameraHit.point;
            }
        }
        
        return position;
    }
    
    // 更新摄像机位置及旋转
    private void UpdateCameraPositionWithRotation()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                // 第一人称摄像机位置旋转更新
                
                // 更新摄像机位置
                _targetPosition = _focusPosition;
                // 旋转摄像机
                _targetRotation = Quaternion.Euler(rightAngle, upAngle, 0).normalized;
                
                // 更新玩家旋转
                UpdatePlayerTargetQuaternion();
                
                break;
            case E_CameraView.ThirdPerson:
                // 第三人称摄像机位置旋转更新
                
                // 计算摄像机旋转后方向 
                CalculateDir();
                
                // 旋转摄像机
                _targetRotation = Quaternion.LookRotation(_dir).normalized;
                
                // 设置左右旋转后摄像机的位置
                _targetPosition = 
                    CheckFocusToTargetPoint(_focusPosition + distance * -_dir);
                
                // 更新玩家旋转
                UpdatePlayerTargetQuaternion();
                break;
            case E_CameraView.ThirdPersonFurther:
                // 更远的第三人称摄像机位置旋转更新
                
                // 计算摄像机旋转后方向 
                CalculateDir();
                
                // 旋转摄像机
                _targetRotation = Quaternion.LookRotation(_dir).normalized;
                
                // 设置左右旋转后摄像机的位置
                _targetPosition =
                    CheckFocusToTargetPoint(_focusPosition + distance * thirdPersonFurtherRatio * -_dir);
                
                // 更新玩家旋转
                UpdatePlayerTargetQuaternion();
                break;
        }
    }
    
    // 更新玩家旋转目标角度(第一人称直接跟随摄像机旋转，第三人称有过程旋转)
    private void UpdatePlayerTargetQuaternion()
    {
        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                // 旋转角色，根据鼠标左右移动旋转
                _playerTargetRotation = Quaternion.Euler(0, upAngle, 0).normalized;
                break;
            case E_CameraView.ThirdPerson:
                // 让玩家移动状态机为不为空时(及非编辑器模式下)||Idle状态下当前摄像机朝向和人物朝向的两向量夹角大于某一角度时人物旋转至摄像机朝向
                if (!(PlayerMovementStateMachine.Instance is null) && PlayerMovementStateMachine.Instance.GetNowStateString() == "Idle")
                {
                    if (CheckPlayerAndCameraAngleInXoz())
                    {
                        // 将人物在Xoz平面的旋转角度通过一定时间旋转成摄像机的角度
                        _playerTargetRotation = Quaternion.LookRotation(_dirXoz).normalized;
                    }
                }
                // 其他状态保持当前摄像机朝向与人物朝向一致，并且将玩家的旋转目标四元数设置为当前玩家旋转四元数，以防止玩家在结束当前状态回到Idle状态后继续旋转发生抽搐
                else
                {
                    // 直接将玩家的绕y轴旋转的欧拉角信息同步成摄像机的旋转信息
                    _playerTargetRotation = Quaternion.LookRotation(_dirXoz).normalized;
                }
                break;
            case E_CameraView.ThirdPersonFurther:
                // 让玩家移动状态机为不为空时(及非编辑器模式下)||Idle状态下当前摄像机朝向和人物朝向的两向量夹角大于某一角度时人物旋转至摄像机朝向
                if (!(PlayerMovementStateMachine.Instance is null) && PlayerMovementStateMachine.Instance.GetNowStateString() == "Idle")
                {
                    if (CheckPlayerAndCameraAngleInXoz())
                    {
                        // 将人物在Xoz平面的旋转角度通过一定时间旋转成摄像机的角度
                        _playerTargetRotation = Quaternion.LookRotation(_dirXoz).normalized;
                    }
                }
                // 其他状态保持当前摄像机朝向与人物朝向一致，并且将玩家的旋转目标四元数设置为当前玩家旋转四元数，以防止玩家在结束当前状态回到Idle状态后继续旋转发生抽搐
                else
                {
                    // 直接将玩家的绕y轴旋转的欧拉角信息同步成摄像机的旋转信息
                    _playerTargetRotation = Quaternion.LookRotation(_dirXoz).normalized;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 计算旋转后摄像机面朝向方向
    private void CalculateDir()
    {
        // 计算摄像机绕聚焦点上下旋转后聚焦点到摄像机的方向向量
        _dir = Quaternion.AngleAxis(rightAngle, Vector3.right) * Vector3.forward;
        // 计算摄像机绕聚焦点左右旋转后聚焦点到摄像机的方向向量
        _dir = Quaternion.AngleAxis(upAngle, Vector3.up) * _dir;
        // 计算摄像机在Xoz平面旋转的方向向量
        _dirXoz = Quaternion.AngleAxis(upAngle, Vector3.up) * Vector3.forward;
        
        _dirXoz = _dirXoz.normalized;
        _dir = _dir.normalized;
    }
    
    // 检查人物Forward向量在Xoz平面与摄像机Forward向量的夹角是否大于最大夹角
    private bool CheckPlayerAndCameraAngleInXoz()
    {
        _playerForwardXoz = Vector3.ProjectOnPlane(_targetTransform.forward, Vector3.up);
        _cameraForwardXoz = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        _xozAngle = Vector3.Angle(_playerForwardXoz, _cameraForwardXoz);
        return _xozAngle > thirdPersonViewCameraWithPlayerMaxAngle || _xozAngle < -thirdPersonViewCameraWithPlayerMaxAngle;
    }
    
    // 玩家四元数向目标四元数旋转
    private IEnumerator PlayerRotateToTargetQuaternion()
    {
        playerIsRotating = true;
        while (!isStopPlayerRotation)
        {
            _targetTransform.rotation = Quaternion.Slerp(_targetTransform.rotation, _playerTargetRotation, playerRotateSpeed).normalized;
            yield return _waitForFixedUpdate;
        }

    }
    
    // 摄像机向目标位置移动和旋转
    private IEnumerator CameraMoveAndRotateToTarget()
    {
        cameraIsMovingAndRotating = true;
        while (!isStopCameraPositionAndRotation)
        {
            transform.position = Vector3.Lerp(transform.position, _targetPosition, cameraFollowSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, cameraRotateSpeed).normalized;
            yield return _waitForFixedUpdate;
        }
    }

    // 立刻更新摄像机位置和旋转至目标位置
    public void UpdateCameraPositionAndRotationImmediately()
    {
        UpdateFocusPosition();

        switch (nowView)
        {
            case E_CameraView.FirstPerson:
                transform.position = _focusPosition;
                // 旋转摄像机
                transform.rotation = Quaternion.Euler(rightAngle, upAngle, 0).normalized;
                break;
            case E_CameraView.ThirdPerson:
                // 计算摄像机旋转后方向 
                CalculateDir();
                
                // 设置摄像机旋转
                transform.rotation = Quaternion.LookRotation(_dir).normalized;
                // 设置摄像机位置
                transform.position = _focusPosition + distance * -_dir;
                break;
            case E_CameraView.ThirdPersonFurther:
                // 计算摄像机旋转后方向 
                CalculateDir();
                
                // 设置摄像机旋转
                transform.rotation = Quaternion.LookRotation(_dir).normalized;
                // 设置摄像机位置
                transform.position = _focusPosition + distance * thirdPersonFurtherRatio * -_dir;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 移动聚焦点到目标位置
    private IEnumerator MoveFocusOffsetToTarget()
    {
        isMoveCameraFocusOffset = true;
        while (Mathf.Abs(thirdPersonFocusWithPlayerOffset.x - focusTargetOffset.x) > 0.01f)
        {
            thirdPersonFocusWithPlayerOffset =
                Vector3.Lerp(thirdPersonFocusWithPlayerOffset, focusTargetOffset, focusMoveRate);
            yield return _waitForFixedUpdate;
        }
        thirdPersonFocusWithPlayerOffset = focusTargetOffset;
        isMoveCameraFocusOffset = false;
    }
    // 开始移动聚焦点协程
    public void StartMoveFocusOffsetToTarget()
    {
        _focusMoveCoroutine = StartCoroutine(MoveFocusOffsetToTarget());
    }
    // 停止移动聚焦点协程
    public void StopMoveFocusOffsetToTarget()
    {
        StopCoroutine(_focusMoveCoroutine);
    }
    
    // 设置并移动第三人称下摄像机与聚焦点之间的偏移量的值为目标值
    public void SetThirdPersonFocusWithPlayerOffsetToTarget(E_State nowState)
    {
        _thirdPersonFocusWithPlayerTargetOffset = listThirdPersonViewFocusOffsetChangeOffset[(int)nowState] + _thirdPersonFocusWithPlayerDefaultOffset;
        if (!_isChangeThirdPersonFocusWithPlayerOffset)
        {
            _changeThirdPersonFocusWithPlayerOffsetCoroutine =
                StartCoroutine(ChangeThirdPersonFocusWithPlayerOffsetToTarget());
        }
    }
    // 重置第三人称下摄像机与聚焦点之间的偏移量的值为目标值
    public void ResetThirdPersonFocusWithPlayerOffsetToDefault()
    {
        _thirdPersonFocusWithPlayerTargetOffset = _thirdPersonFocusWithPlayerDefaultOffset;
        if (!_isChangeThirdPersonFocusWithPlayerOffset)
        {
            _changeThirdPersonFocusWithPlayerOffsetCoroutine =
                StartCoroutine(ChangeThirdPersonFocusWithPlayerOffsetToTarget());
        }
    }
    // 改变第三人称下摄像机与聚焦点的偏移量的值的协程
    private IEnumerator ChangeThirdPersonFocusWithPlayerOffsetToTarget()
    {
        _isChangeThirdPersonFocusWithPlayerOffset = true;
        
        while (Vector3.Distance(thirdPersonFocusWithPlayerOffset, _thirdPersonFocusWithPlayerTargetOffset) > 0.01f)
        {
            thirdPersonFocusWithPlayerOffset = Vector3.Lerp(thirdPersonFocusWithPlayerOffset,
                _thirdPersonFocusWithPlayerTargetOffset, thirdPersonFocusWithPlayerOffsetChangeRate);
            yield return _waitForFixedUpdate;
        }

        thirdPersonFocusWithPlayerOffset = _thirdPersonFocusWithPlayerTargetOffset;
        _isChangeThirdPersonFocusWithPlayerOffset = false;
    }
    
    // 使摄像机始终追踪怪物
    private void ChangeCameraToFollowMonster()
    {
        isFollowMonster = true;
        nowFollowMonster = GameManager.Instance.lastTimeNearestMonster;
        nowFollowMonster.isMainCameraFollowing = true;
    }

    // 恢复摄像机去追踪人物
    public void ResetCameraToFollowPlayer()
    {
        rightAngle = 4.5f;
        
        upAngle = Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.forward);
        if (Vector3.Dot(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.right) < 0f)
        {
            upAngle = -upAngle;
        }
        isFollowMonster = false;
        nowFollowMonster.isMainCameraFollowing = false;
    }
}