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

    private GrapplingHookGears _grapplingHookGears;
    
    
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
                midCheckPoint.position =
                    UICameraController.Instance.GetRelativeMainCameraToUICameraPosition(_grapplingHookGears
                        .CameraForwardRaycastHit.point);
                
                midCheckPoint.LookAt(UICameraController.Instance.transform);
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
                leftCheckPoint.position =
                    UICameraController.Instance.GetRelativeMainCameraToUICameraPosition(_grapplingHookGears
                        .LeftHookRaycastHit.point);
                
                leftCheckPoint.LookAt(UICameraController.Instance.transform);
                leftCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                leftCheckPoint.gameObject.SetActive(false);
            }
        
            if (_grapplingHookGears.hasAutoRightGrapplingHookCheckPoint)
            {
                rightCheckPoint.position =
                    UICameraController.Instance.GetRelativeMainCameraToUICameraPosition(_grapplingHookGears
                        .RightHookRaycastHit.point);
                
                rightCheckPoint.LookAt(UICameraController.Instance.transform);
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
