using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.Serialization;

public class GrapplingHookGears : Equipment
{ 
    public Transform grapplingHookGearLeftModelTransform;
    public Vector3 grapplingHookGearLeftOnEquipPositionOffset;
    private Vector3 _grapplingHookGearLeftOnItemPositionOffset;
    private Quaternion _grapplingHookGearLeftOnItemRotationOffset;
    public Transform grapplingHookGearRightModelTransform;
    public Vector3 grapplingHookGearRightOnEquipPositionOffset;
    private Vector3 _grapplingHookGearRightOnItemPositionOffset;
    private Quaternion _grapplingHookGearRightOnItemRotationOffset;
    [HideInInspector] public Transform grapplingHookGearModelFatherTransform;
    
    public GrapplingHook grapplingHookLeft;
    public GrapplingHook grapplingHookRight;
    
    [Header("钩锁信息")]
    // 从摄像机出发检测可以进行钩锁勾爪勾住的最大距离
    public float maxHookCheckDistance = 10f;
    // 从摄像机射出射线进行球形检测半径大小
    public float cameraHookCheckPointSphereCastRadius = 1f;
    // 从立体机动装置射出球形射线对两边进行自动寻找勾住点的半径大小
    public float autoHookCheckPointSphereCastRadius = 1f;
    // 钩锁绳子基础长度
    public float grapplingHookRopeLength = 10f;
    // 钩锁绳子拉长系数
    public float grapplingHookRopeMaxLengthRatio = 1.2f;
    // 钩锁绳子缩短系数
    public float grapplingHookRopeMinLengthRatio = 0f;
    // 钩锁自动回收系数
    public float grapplingHookRopeDestroyLengthRatio = 1.3f;
    // 钩锁自动寻找勾中点方向
    public Vector3 autoHookCheckPointSphereDirection = new Vector3(1, 1, 1);
    
    // 左右钩锁发射器弹簧节点
    public SpringJoint[] springJoints;
    // 是否存在可供钩锁勾中的位置
    [HideInInspector] public bool hasHookCheckPoint;
    // 钩锁勾中的位置
    [HideInInspector] public Vector3 hookCheckPoint;
    // 左钩锁发射器是否自动找到了勾中点
    [HideInInspector] public bool hasAutoLeftGrapplingHookCheckPoint;
    // 左钩锁发射器自动找到的勾中点坐标
    [HideInInspector] public Vector3 leftGrapplingHookCastHitHookCheckPoint;
    // 右钩锁发射器是否自动找到了勾中点
    [HideInInspector] public bool hasAutoRightGrapplingHookCheckPoint;
    // 右钩锁发射器自动找到的勾中点坐标
    [HideInInspector] public Vector3 rightGrapplingHookCastHitHookCheckPoint;
    // 钩锁目标最大长度
    [HideInInspector] public float grapplingHookMaxLength;
    // 钩锁目标最小长度
    [HideInInspector] public float grapplingHookMinLength;
    // 钩锁自动断开长度
    [HideInInspector] public float grapplingHookRopeDestroyLength;
    // 从摄像机坐标向摄像机面朝向向量方向发射用来检测是否存在可供钩锁勾中的图层的射线的击中信息(先射线检测若是不存在击中点则使用球形检测)
    private RaycastHit _cameraForwardRaycastHit;
    // 左钩锁向从摄像机发射射线击中点发射的射线的击中信息
    private RaycastHit _leftHookRaycastHit;
    // 右钩锁向从摄像机发射射线击中点发射的射线的击中信息
    private RaycastHit _rightHookRaycastHit;
    // 用来计算钩锁发射器距离勾中点距离
    private float _grapplingHookDistance;
    // 从左边立体机动装置射出球形射线检测的方向
    private Vector3 _leftAutoHookCheckPointSphereDirection;
    // 从右边立体机动装置射出球形射线检测的方向
    private Vector3 _rightAutoHookCheckPointSphereDirection;
    // 用来临时计数的变量
    private int _count;
    // 钩锁状态引用
    private Grapple _grappleState;
    private void Awake()
    {
        equipmentName = E_EquipmentName.GrapplingHookGears;
        equipmentType = E_EquipmentType.MovementEquipment;

        _grapplingHookGearLeftOnItemPositionOffset = grapplingHookGearLeftModelTransform.localPosition;
        _grapplingHookGearLeftOnItemRotationOffset = grapplingHookGearLeftModelTransform.rotation;
        _grapplingHookGearRightOnItemPositionOffset = grapplingHookGearRightModelTransform.localPosition;
        _grapplingHookGearRightOnItemRotationOffset = grapplingHookGearRightModelTransform.rotation;

        springJoints = new SpringJoint[2];
        
        grapplingHookGearModelFatherTransform = transform;
    }

    public override bool PickUpItem()
    {
        if (!(EquipmentsController.Instance is null))
        {
            controller = EquipmentsController.Instance;
            _grappleState = controller.playerMovementStateMachine.GrappleState;
            controller.AddEquipment(this);
            
            grapplingHookGearLeftModelTransform.gameObject.SetActive(true);
            grapplingHookGearRightModelTransform.gameObject.SetActive(true);
        }
        
        // 初始化钩锁相关参数
        hasHookCheckPoint = false;
        hasAutoLeftGrapplingHookCheckPoint = false;
        hasAutoRightGrapplingHookCheckPoint = false;
        
        Debug.Log(111111111);
        grapplingHookLeft.gameObject.SetActive(true);
        grapplingHookRight.gameObject.SetActive(true);
        
        // 初始化两钩锁发射器
        grapplingHookLeft.isLeft = true;
        grapplingHookRight.isLeft = false;
        
        AddSpringJointComponent();
        
        return true;
    }

    public override bool DiscardItem()
    {
        // 设置左右两个立体机动装置模型Transform父节点及位置
        grapplingHookGearLeftModelTransform.SetParent(transform);
        grapplingHookGearLeftModelTransform.localPosition = _grapplingHookGearLeftOnItemPositionOffset;
        grapplingHookGearLeftModelTransform.localRotation = _grapplingHookGearLeftOnItemRotationOffset;
        grapplingHookGearRightModelTransform.SetParent(transform);
        grapplingHookGearRightModelTransform.localPosition = _grapplingHookGearRightOnItemPositionOffset;
        grapplingHookGearRightModelTransform.localRotation = _grapplingHookGearRightOnItemRotationOffset;
        
        grapplingHookGearModelFatherTransform = transform;

        transform.rotation = controller.playerFootTransform.rotation;
        transform.position = controller.playerFootTransform.position;
        RemoveSpringJointComponent();
        grapplingHookGearLeftModelTransform.gameObject.SetActive(false);
        grapplingHookGearRightModelTransform.gameObject.SetActive(false);
        return true;
    }

    public override bool UseItem()
    {
        
        return true;
    }

    public override bool WearEquipment()
    {
        if (grapplingHookGearModelFatherTransform == transform)
        {
            // 设置左右两个立体机动装置模型Transform父节点及位置
            grapplingHookGearLeftModelTransform.SetParent(controller.grapplingHookGearsFatherTransform);
            grapplingHookGearLeftModelTransform.localRotation = Quaternion.identity;
            grapplingHookGearLeftModelTransform.localPosition = grapplingHookGearLeftOnEquipPositionOffset;
            
            grapplingHookGearRightModelTransform.SetParent(controller.grapplingHookGearsFatherTransform);
            grapplingHookGearRightModelTransform.localRotation = Quaternion.identity;
            grapplingHookGearRightModelTransform.localPosition = grapplingHookGearRightOnEquipPositionOffset;
            
            grapplingHookGearModelFatherTransform = controller.grapplingHookGearsFatherTransform;
        }
        
        _grappleState.grapplingHookGears = this;
        
        grapplingHookGearLeftModelTransform.gameObject.SetActive(true);
        grapplingHookGearRightModelTransform.gameObject.SetActive(true);

        return true;
    }

    public override bool RemoveEquipment()
    {
        grapplingHookGearLeftModelTransform.gameObject.SetActive(false);
        grapplingHookGearRightModelTransform.gameObject.SetActive(false);

        return true;
    }

    public override void ListenEquipmentUse()
    {
        // 更新钩锁勾中点
        UpdateHasHookCheckPoint();

        // 监听钩锁发射控制
        ListenGrapplingHookShoot();
    }
    
    
    // 由摄像机向前发射是否存在可以勾中的位置，若摄像机未对准可以勾中的位置由钩锁发射器自行对两边进行查找
    private void UpdateHasHookCheckPoint()
    {
        // 当两个钩锁发射器均处于发射钩锁或钩锁勾中状态或回收钩锁时不需要更新勾中点
        if (grapplingHookLeft.isDrawHookAndRope && grapplingHookRight.isDrawHookAndRope)
        {
            hasHookCheckPoint = false;
            return;
        }

        // 摄像机向前射线检测
        hasHookCheckPoint = Physics.Raycast(
            CameraController.Instance.transform.position,
            CameraController.Instance.transform.forward,
            out _cameraForwardRaycastHit,
            maxHookCheckDistance,
            InfoManager.Instance.layerGrapplingHookCheck);

        // 射线检测检测不到击中点时使用球形射线对两个图层检测
        if (!hasHookCheckPoint)
        {
            hasHookCheckPoint = Physics.SphereCast(
                CameraController.Instance.transform.position,
                cameraHookCheckPointSphereCastRadius,
                CameraController.Instance.transform.forward,
                out _cameraForwardRaycastHit,
                maxHookCheckDistance - cameraHookCheckPointSphereCastRadius,
                InfoManager.Instance.layerGrapplingHookCheck);
        }
        
        // 画画
        Debug.DrawLine(CameraController.Instance.transform.position,
            CameraController.Instance.transform.position + CameraController.Instance.transform.forward * maxHookCheckDistance,
            Color.cyan);

        // 当摄像机前不存在勾中点两个钩锁发射器中未发射钩锁的自行向两边寻找是否存在钩锁勾中点
        if (!hasHookCheckPoint)
        {
            // 更新自动寻找勾中点的射线方向
            _leftAutoHookCheckPointSphereDirection =
                (-controller.playerTransform.right * autoHookCheckPointSphereDirection.x +
                 controller.playerTransform.up * autoHookCheckPointSphereDirection.y +
                 controller.playerTransform.forward * autoHookCheckPointSphereDirection.z).normalized;
            _rightAutoHookCheckPointSphereDirection =
                (controller.playerTransform.right * autoHookCheckPointSphereDirection.x +
                 controller.playerTransform.up * autoHookCheckPointSphereDirection.y +
                 controller.playerTransform.forward * autoHookCheckPointSphereDirection.z).normalized;

            // 左钩锁发射器不是发射过去的过程中和勾中了的锁定状态和回收状态，未左钩锁发射器自动寻找勾中点
            if (!grapplingHookLeft.isDrawHookAndRope)
            {
                hasAutoLeftGrapplingHookCheckPoint = Physics.SphereCast(
                    grapplingHookLeft.hookShootPoint.position,
                    autoHookCheckPointSphereCastRadius,
                    _leftAutoHookCheckPointSphereDirection,
                    out _leftHookRaycastHit,
                    maxHookCheckDistance - autoHookCheckPointSphereCastRadius,
                    InfoManager.Instance.layerGrapplingHookCheck);

                if (hasAutoLeftGrapplingHookCheckPoint)
                {
                    leftGrapplingHookCastHitHookCheckPoint = _leftHookRaycastHit.point;
                }
                
                // 画画
                Debug.DrawLine(grapplingHookLeft.hookShootPoint.position,
                    grapplingHookLeft.hookShootPoint.position + _leftAutoHookCheckPointSphereDirection * maxHookCheckDistance,
                    Color.cyan);
            }
            else
            {
                hasAutoLeftGrapplingHookCheckPoint = false;
            }

            // 右钩锁发射器不是发射过去的过程中和锁定状态，未左钩锁发射器自动寻找勾中点
            if (!grapplingHookRight.isDrawHookAndRope)
            {
                hasAutoRightGrapplingHookCheckPoint = Physics.SphereCast(
                    grapplingHookRight.hookShootPoint.position,
                    autoHookCheckPointSphereCastRadius,
                    _rightAutoHookCheckPointSphereDirection,
                    out _rightHookRaycastHit,
                    maxHookCheckDistance - autoHookCheckPointSphereCastRadius,
                    InfoManager.Instance.layerGrapplingHookCheck);

                if (hasAutoRightGrapplingHookCheckPoint)
                {
                    rightGrapplingHookCastHitHookCheckPoint = _rightHookRaycastHit.point;
                }
                
                // 画画
                Debug.DrawLine(grapplingHookRight.hookShootPoint.position,
                    grapplingHookRight.hookShootPoint.position + _rightAutoHookCheckPointSphereDirection * maxHookCheckDistance,
                    Color.cyan);
            }
            else
            {
                hasAutoRightGrapplingHookCheckPoint = false;
            }
        }
        else
        {
            hookCheckPoint = _cameraForwardRaycastHit.point;
            hasAutoLeftGrapplingHookCheckPoint = false;
            hasAutoRightGrapplingHookCheckPoint = false;
        }
    }

    // 当摄像机前存在勾中点，指定发射钩锁的发射器，由钩锁发射器向这点发射射线更新勾中点坐标，当是自动勾中点更新勾中点为左右自动勾中点其一
    private void UpdateHookCheckPoint(bool isLeft)
    {
        if (!hasHookCheckPoint)
        {
            hookCheckPoint = isLeft ? leftGrapplingHookCastHitHookCheckPoint : rightGrapplingHookCastHitHookCheckPoint;
            return;
        }

        if (isLeft)
        {
            _grapplingHookDistance =
                Vector3.Distance(grapplingHookLeft.hookShootPoint.position, _cameraForwardRaycastHit.point);
            if (Physics.Raycast(grapplingHookLeft.hookShootPoint.position,
                    _cameraForwardRaycastHit.point - grapplingHookLeft.hookShootPoint.position,
                    out _leftHookRaycastHit,
                    _grapplingHookDistance,
                    InfoManager.Instance.layerGrapplingHookCheck
                ))
            {
                hookCheckPoint = _leftHookRaycastHit.point;
            }

            // 画画
            Debug.DrawLine(grapplingHookLeft.hookShootPoint.position,
                _cameraForwardRaycastHit.point,
                Color.cyan);
        }
        else
        {
            _grapplingHookDistance =
                Vector3.Distance(grapplingHookRight.hookShootPoint.position, _cameraForwardRaycastHit.point);
            if (Physics.Raycast(grapplingHookRight.hookShootPoint.position,
                    _cameraForwardRaycastHit.point - grapplingHookRight.hookShootPoint.position,
                    out _rightHookRaycastHit,
                    _grapplingHookDistance,
                    InfoManager.Instance.layerGrapplingHookCheck
                ))
            {
                hookCheckPoint = _rightHookRaycastHit.point;
            }

            // 画画
            Debug.DrawLine(grapplingHookRight.hookShootPoint.position,
                _cameraForwardRaycastHit.point,
                Color.cyan);
        }
    }
    
    // 监听钩锁发射与回收
    private void ListenGrapplingHookShoot()
    {
        
        // 收回钩锁
        if (controller.equipmentUseInputInfo.AimInput)
        {
            if (grapplingHookLeft.IsGrapplingHookLocked())
            {
                grapplingHookLeft.RetractRope();
            }

            if (grapplingHookRight.IsGrapplingHookLocked())
            {
                grapplingHookRight.RetractRope();
            }
            
        }
        
        // 人拉向钩锁
        if (controller.equipmentUseInputInfo.FireInput &&
            (grapplingHookLeft.IsGrapplingHookLocked() &&
             (controller.playerMovementStateMachine.CurrentState.state != E_State.Grapple || 
              !_grappleState.IsMoveToLeftHookCheckPoint)
             ||
             (grapplingHookRight.IsGrapplingHookLocked() &&
              (controller.playerMovementStateMachine.CurrentState.state != E_State.Grapple ||
               !_grappleState.IsMoveToRightHookCheckPoint))))
        {
            if (controller.playerMovementStateMachine.CurrentState.state != E_State.Grapple)
            {
                if (controller.playerMovementStateMachine.ChangeState(_grappleState))
                {
                    return;
                }
            }
            else
            {
                if (!_grappleState.IsMoveToLeftHookCheckPoint || !_grappleState.IsMoveToRightHookCheckPoint)
                {
                    _grappleState.SetVelocity();
                }
            }
        }
        
        if (!hasHookCheckPoint && !hasAutoLeftGrapplingHookCheckPoint && !hasAutoRightGrapplingHookCheckPoint)
        {
            return;
        }

        if (controller.equipmentUseInputInfo.HookShootLeftInput && !grapplingHookLeft.isDrawHookAndRope &&
            (hasHookCheckPoint || hasAutoLeftGrapplingHookCheckPoint))
        {
            UpdateHookCheckPoint(true);
            grapplingHookLeft.ShootHook(hookCheckPoint);
        }

        if (controller.equipmentUseInputInfo.HookShootRightInput && !grapplingHookRight.isDrawHookAndRope &&
            (hasHookCheckPoint || hasAutoRightGrapplingHookCheckPoint))
        {
            UpdateHookCheckPoint(false);
            grapplingHookRight.ShootHook(hookCheckPoint);
        }
    }
    
    // 初始化弹簧组件
    public void InitPlayerSpringJoint(int index)
    {
        springJoints[index].maxDistance = float.PositiveInfinity;
        springJoints[index].minDistance = 0f;
    }
    
    // 为人物添加弹簧组件
    public void AddSpringJointComponent()
    {
        for (_count = 0; _count < 2; _count++)
        {
            springJoints[_count] = controller.playerTransform.AddComponent<SpringJoint>();
        }
        
        // 初始化弹簧关节参数
        grapplingHookMaxLength = grapplingHookRopeLength * grapplingHookRopeMaxLengthRatio;
        grapplingHookMinLength = grapplingHookRopeLength * grapplingHookRopeMinLengthRatio;
        grapplingHookRopeDestroyLength = grapplingHookRopeLength * grapplingHookRopeDestroyLengthRatio;
        
        for (_count = 0; _count < springJoints.Length; _count++)
        {
            springJoints[_count].autoConfigureConnectedAnchor = false;
            springJoints[_count].spring = 4.5f;
            springJoints[_count].damper = 1f;
            springJoints[_count].massScale = 4.5f;
            InitPlayerSpringJoint(_count);
        }

        grapplingHookLeft.springJoint = springJoints[grapplingHookLeft.isLeft ? 0 : 1];
        grapplingHookRight.springJoint = springJoints[grapplingHookRight.isLeft ? 0 : 1];
    }
    // 移除人物身上弹簧组件
    public void RemoveSpringJointComponent()
    {
        for (_count = 0; _count < 2; _count++)
        {
            GameObject.Destroy(springJoints[_count]);
        }
    }
}
