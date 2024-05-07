using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GrapplingHookCheckPointController : MonoBehaviour
{
    public Transform screenCenterPoint;
    
    public Transform midCheckPoint;
    public Transform leftCheckPoint;
    public Transform rightCheckPoint;

    public Camera uiCamera;

    private GrapplingHookGears _grapplingHookGears;

    // 临时保存计算击中点在UICamera坐标系下的相对方向
    private Vector3 _relativeDirection;
    
    private void Awake()
    {
        midCheckPoint.gameObject.SetActive(false);
        leftCheckPoint.gameObject.SetActive(false);
        rightCheckPoint.gameObject.SetActive(false);
        
        screenCenterPoint.gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        if (!(EquipmentsController.Instance is null) && 
            EquipmentsController.Instance.nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] != -1 &&
            EquipmentsController.Instance.nowEquipments[(int)E_EquipmentType.Weapon].equipmentName == E_EquipmentName.GrapplingHookGears)
        {
            _grapplingHookGears = EquipmentsController.Instance.nowEquipments[(int)E_EquipmentType.Weapon] as GrapplingHookGears;
            if (_grapplingHookGears.hasHookCheckPoint)
            {
                // 设置提示点为击中点在主摄像机的相对位置在UI摄像机的相对位置
                _relativeDirection = uiCamera.transform
                    .InverseTransformDirection(CameraController.Instance.transform.InverseTransformDirection(
                        _grapplingHookGears.CameraForwardRaycastHit.point -
                        CameraController.Instance.transform.position).normalized).normalized;
                
                midCheckPoint.position = 
                    _grapplingHookGears.CameraForwardRaycastHit.distance *
                    _relativeDirection +
                    uiCamera.transform.position;
                
                midCheckPoint.LookAt(uiCamera.transform);
                midCheckPoint.gameObject.SetActive(true);
                screenCenterPoint.gameObject.SetActive(false);
            }
            else
            {
                midCheckPoint.gameObject.SetActive(false);
                screenCenterPoint.gameObject.SetActive(true);
            }
        
            if (_grapplingHookGears.hasAutoLeftGrapplingHookCheckPoint)
            {
                // 设置提示点为击中点在主摄像机的相对位置在UI摄像机的相对位置
                _relativeDirection = uiCamera.transform
                    .InverseTransformDirection(CameraController.Instance.transform.InverseTransformDirection(
                        _grapplingHookGears.LeftHookRaycastHit.point -
                        CameraController.Instance.transform.position).normalized).normalized;

                leftCheckPoint.position =
                    Vector3.Distance(CameraController.Instance.transform.position,
                        _grapplingHookGears.LeftHookRaycastHit.point) *
                    _relativeDirection +
                    uiCamera.transform.position;
                
                leftCheckPoint.LookAt(uiCamera.transform);
                leftCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                leftCheckPoint.gameObject.SetActive(false);
            }
        
            if (_grapplingHookGears.hasAutoRightGrapplingHookCheckPoint)
            {
                // 设置提示点为击中点在主摄像机的相对位置在UI摄像机的相对位置
                _relativeDirection = uiCamera.transform
                    .InverseTransformDirection(CameraController.Instance.transform.InverseTransformDirection(
                        _grapplingHookGears.RightHookRaycastHit.point -
                        CameraController.Instance.transform.position).normalized).normalized;
                
                rightCheckPoint.position = 
                    Vector3.Distance(CameraController.Instance.transform.position,
                        _grapplingHookGears.RightHookRaycastHit.point) *
                    _relativeDirection +
                    uiCamera.transform.position;
                
                rightCheckPoint.LookAt(uiCamera.transform);
                rightCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                rightCheckPoint.gameObject.SetActive(false);
            }
        }
        else
        {
            midCheckPoint.gameObject.SetActive(false);
            leftCheckPoint.gameObject.SetActive(false);
            rightCheckPoint.gameObject.SetActive(false);
        }
    }
}
