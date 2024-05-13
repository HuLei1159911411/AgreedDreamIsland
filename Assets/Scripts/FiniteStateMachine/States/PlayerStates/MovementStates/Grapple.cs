using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class Grapple : BaseState
{
    private PlayerMovementStateMachine _movementStateMachine;
    // 计算从钩锁起始点到目标点所需提供速度的大小时所用到的参数
    private Vector3 _velocityY;
    private Vector3 _velocityXZ;
    private float _differenceHighStartAndEnd;
    private float _gravity;
    private float _distanceStartAndEndX;
    private float _distanceStartAndEndZ;
    private float _moveTime;
    // 在为速度赋值时临时用来保存计算结果的变量
    private Vector3 _calculateVelocity;

    // 是否正在飞向目标点
    public bool IsMoveToLeftHookCheckPoint;
    public bool IsMoveToRightHookCheckPoint;
    // 是否正在进行钩锁回收的转动动作
    public bool IsGrappleHookRetractLeft;
    public bool IsGrappleHookRetractRight;
    // 监听是否离开过地面
    public bool IsLeftGround;
    // 需要添加到状态机中在发生碰撞时需要调用的函数
    private Func<bool> _whenCollisionEnterFunc;
    // 钩锁装置(在装置使用时会将自行找到该状态并对该值赋值)
    public GrapplingHookGears grapplingHookGears;
    
    public Grapple(StateMachine stateMachine) : base(E_State.Grapple, stateMachine)
    {
        if (stateMachine is PlayerMovementStateMachine)
        {
            _movementStateMachine = stateMachine as PlayerMovementStateMachine;
        }

        _whenCollisionEnterFunc = CheckIsMovedToHookCheckPoint;
    }

    public override void Enter()
    {
        base.Enter();
        IsLeftGround = false;
        _movementStateMachine.WhenOnCollisionEnter += _whenCollisionEnterFunc;
        
        _movementStateMachine.playerAnimator.SetTrigger(_movementStateMachine.DicAnimatorIndexes["ToGrapple"]);
        _gravity = Mathf.Abs(Physics.gravity.y);
        SetVelocity();
        
        // 防止进行钩锁自动进行旋转
        if (IsGrappleHookRetractLeft || IsGrappleHookRetractRight)
        {
            InitGrappleHookTurnParameters();
        }
        
        // 设置状态机最大速度为钩锁的最大速度
        _movementStateMachine.nowMoveSpeed = _movementStateMachine.grappleSpeed;
    }

    public override void Exit()
    {
        base.Exit();

        _movementStateMachine.WhenOnCollisionEnter -= _whenCollisionEnterFunc;
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();

        if (!IsLeftGround && !_movementStateMachine.isOnGround)
        {
            IsLeftGround = true;
        }
        
        // 两个绳索同时处于收回状态(监听钩锁是否是收回状态驱动去做转身动作)
        if (grapplingHookGears.grapplingHookLeft.isRetractingRope && grapplingHookGears.grapplingHookRight.isRetractingRope)
        {
            // 提前结束移动到勾中点的状态
            IsMoveToLeftHookCheckPoint = false;
            IsMoveToRightHookCheckPoint = false;

            // 当人物当前没在地面时播放旋转动画
            if (!_movementStateMachine.isOnGround)
            {
                if (!IsGrappleHookRetractLeft && !IsGrappleHookRetractRight)
                {
                    IsGrappleHookRetractRight = true;
                    IsGrappleHookRetractLeft = true;
                    _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractBoth"], true);
                }
                else
                {
                    if (!IsGrappleHookRetractLeft)
                    {
                        IsGrappleHookRetractLeft = true;
                        _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractLeft"], true);
                    }
                    if (!IsGrappleHookRetractRight)
                    {
                        IsGrappleHookRetractRight = true;
                        _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractRight"], true);
                    }
                }
            }
        }
        // 单个钩锁处于回收状态
        else
        {
            if (grapplingHookGears.grapplingHookLeft.isRetractingRope && !IsGrappleHookRetractLeft)
            {
                IsMoveToLeftHookCheckPoint = false;

                // 当正在地上或正在进行相反方向的旋转时不进行旋转
                if (!_movementStateMachine.isOnGround && !IsGrappleHookRetractRight)
                {
                    IsGrappleHookRetractLeft = true;
                    _movementStateMachine.playerAnimator.SetBool(
                        _movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractLeft"], true);
                }
            }

            if (grapplingHookGears.grapplingHookRight.isRetractingRope && !IsGrappleHookRetractRight)
            {
                IsMoveToRightHookCheckPoint = false;

                if (!_movementStateMachine.isOnGround && !IsGrappleHookRetractLeft)
                {
                    IsGrappleHookRetractRight = true;
                    _movementStateMachine.playerAnimator.SetBool(
                        _movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractRight"], true);
                }
            }
        }

        if (ListenStateChange())
        {
            return;
        }

        ListenInputMove();
    }

    public override void UpdatePhysic()
    {
        base.UpdatePhysic();
        
        _movementStateMachine.ClampXozVelocity();
    }

    // 计算从钩锁起始点到目标点所需提供速度的大小
    private Vector3 CalculateJumpVelocity(Vector3 start, Vector3 end, float differenceHighSecondAndHighest)
    {
        _differenceHighStartAndEnd = end.y - start.y - _movementStateMachine.playerHeight * 0.5f;
        if (start.y < end.y)
        {
            differenceHighSecondAndHighest += Mathf.Abs(_differenceHighStartAndEnd);
        }
        
        _distanceStartAndEndX = end.x - start.x;
        _distanceStartAndEndZ = end.z - start.z;
        
        _velocityY = new Vector3(0, Mathf.Sqrt(2 * _gravity * differenceHighSecondAndHighest), 0);
        
        _moveTime = (_velocityY.y / _gravity) +
                    (Mathf.Sqrt(2 * (differenceHighSecondAndHighest - _differenceHighStartAndEnd) / _gravity));
        
        _velocityXZ = new Vector3(_distanceStartAndEndX / _moveTime, 0, _distanceStartAndEndZ / _moveTime);
        
        return _velocityY + _velocityXZ * _movementStateMachine.grapplingHookRopeMoveVelocityRatio;
    }

    // 设置钩锁拉人时的速度
    public void SetVelocity()
    {
        _movementStateMachine.playerRigidbody.velocity = Vector3.zero;

        // 两个钩锁
        if (grapplingHookGears.grapplingHookLeft.IsGrapplingHookLocked() &&
            !IsMoveToLeftHookCheckPoint &&
            grapplingHookGears.grapplingHookRight.IsGrapplingHookLocked() &&
            !IsMoveToRightHookCheckPoint)
        {
            _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                _movementStateMachine.playerTransform.position,
                (grapplingHookGears.grapplingHookLeft.TargetPoint +
                 grapplingHookGears.grapplingHookRight.TargetPoint) * 0.5f,
                _movementStateMachine.grapplingHighestPointRelativeHigh);
            
            /*
            // 左边的比较高
            if (_movementStateMachine.leftGrapplingHook.TargetPoint.y >
                _movementStateMachine.rightGrapplingHook.TargetPoint.y)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position,
                    _movementStateMachine.leftGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                
                _calculateVelocity = CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position, 
                    _movementStateMachine.rightGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);

                _movementStateMachine.playerRigidbody.velocity += 
                    new Vector3(_calculateVelocity.x, 0, _calculateVelocity.z);
            }
            // 右边的比较高
            else
            {
                _calculateVelocity = _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position,
                    _movementStateMachine.leftGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);

                _movementStateMachine.playerRigidbody.velocity +=
                    new Vector3(_calculateVelocity.x, 0, _calculateVelocity.z);
                
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    _movementStateMachine.leftGrapplingHook.hookShootPoint.position, 
                    _movementStateMachine.rightGrapplingHook.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
            }
            */
            
            IsMoveToLeftHookCheckPoint = true;
            IsMoveToRightHookCheckPoint = true;
        }
        // 单个钩锁
        else
        {
            if (grapplingHookGears.grapplingHookLeft.IsGrapplingHookLocked() &&
                !IsMoveToLeftHookCheckPoint)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    grapplingHookGears.grapplingHookLeft.hookShootPoint.position,
                    grapplingHookGears.grapplingHookLeft.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                IsMoveToLeftHookCheckPoint = true;
            }

            if (grapplingHookGears.grapplingHookRight.IsGrapplingHookLocked() &&
                !IsMoveToRightHookCheckPoint)
            {
                _movementStateMachine.playerRigidbody.velocity += CalculateJumpVelocity(
                    grapplingHookGears.grapplingHookLeft.hookShootPoint.position, 
                    grapplingHookGears.grapplingHookRight.TargetPoint,
                    _movementStateMachine.grapplingHighestPointRelativeHigh);
                IsMoveToRightHookCheckPoint = true;
            }
        }
    }
    
    // 监听状态切换
    private bool ListenStateChange()
    {
        if (!IsMoveToLeftHookCheckPoint && !IsMoveToRightHookCheckPoint && !IsGrappleHookRetractLeft && !IsGrappleHookRetractRight)
        {
            if (_movementStateMachine.isOnGround)
            {
                if (grapplingHookGears.grapplingHookLeft.isDrawHookAndRope)
                {
                    grapplingHookGears.grapplingHookLeft.RetractRope();
                }

                if (grapplingHookGears.grapplingHookRight.isDrawHookAndRope)
                {
                    grapplingHookGears.grapplingHookRight.RetractRope();
                }
                
                if (_movementStateMachine.isFastToRun)
                {
                    if (_movementStateMachine.ChangeState(_movementStateMachine.RunState))
                    {
                        return true;
                    }
                }
                else
                {
                    if (_movementStateMachine.ChangeState(_movementStateMachine.WalkState))
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (_movementStateMachine.ChangeState(_movementStateMachine.FallState))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    // 监听输入控制移动
    private void ListenInputMove()
    {
        // 在钩锁状态下移动加力控制
        if (_movementStateMachine.MoveInputInfo.VerticalInput != 0)
        {
            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.playerTransform.forward *
                                                           (_movementStateMachine.MoveInputInfo.VerticalInput *
                                                            _movementStateMachine.grapplingForce *
                                                            Time.deltaTime));
        }

        if (_movementStateMachine.MoveInputInfo.HorizontalInput != 0)
        {
            _movementStateMachine.playerRigidbody.AddForce(_movementStateMachine.playerTransform.right *
                                                           (_movementStateMachine.MoveInputInfo.HorizontalInput *
                                                            _movementStateMachine.grapplingForce *
                                                            Time.deltaTime));
        }
    }
    
    // 当碰撞发生时调用的函数，当返回值为true时在事件中移除该委托
    private bool CheckIsMovedToHookCheckPoint()
    {
        if (_movementStateMachine.GrappleState.IsLeftGround)
        {
            if (IsMoveToLeftHookCheckPoint)
            {
                IsMoveToLeftHookCheckPoint = false;
            }

            if (IsMoveToRightHookCheckPoint)
            {
                IsMoveToRightHookCheckPoint = false;
            }

            if (IsGrappleHookRetractLeft || IsGrappleHookRetractRight)
            {
                InitGrappleHookTurnParameters();
            }
        }
        
        return false;
    }
    
    // 动画事件---------------------
    // 初始化在钩锁过程中进行钩锁拉过去进行旋转动作的控制参数
    public void InitGrappleHookTurnParameters()
    {
        IsGrappleHookRetractLeft = false;
        IsGrappleHookRetractRight = false;
        _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractLeft"], false);
        _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractRight"], false);
        _movementStateMachine.playerAnimator.SetBool(_movementStateMachine.DicAnimatorIndexes["IsGrappleHookRetractBoth"], false);
    }
}
