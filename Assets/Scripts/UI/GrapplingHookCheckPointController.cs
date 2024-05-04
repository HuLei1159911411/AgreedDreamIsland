using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookCheckPointController : MonoBehaviour
{
    public Transform midCheckPoint;
    public Transform leftCheckPoint;
    public Transform rightCheckPoint;

    private GrapplingHookGears _grapplingHookGears;

    private void Awake()
    {
        midCheckPoint.gameObject.SetActive(false);
        leftCheckPoint.gameObject.SetActive(false);
        rightCheckPoint.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!(EquipmentsController.Instance is null) && 
            EquipmentsController.Instance.nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] != -1 &&
            EquipmentsController.Instance.nowEquipments[(int)E_EquipmentType.Weapon].equipmentName == E_EquipmentName.GrapplingHookGears)
        {
            _grapplingHookGears = EquipmentsController.Instance.nowEquipments[(int)E_EquipmentType.Weapon] as GrapplingHookGears;
            if (_grapplingHookGears.hasHookCheckPoint)
            {
                midCheckPoint.position = _grapplingHookGears.hookCheckPoint;
                midCheckPoint.LookAt(CameraController.Instance.transform);
                midCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                midCheckPoint.gameObject.SetActive(false);
            }
        
            if (_grapplingHookGears.hasAutoLeftGrapplingHookCheckPoint)
            {
                leftCheckPoint.position = _grapplingHookGears.leftGrapplingHookCastHitHookCheckPoint;
                leftCheckPoint.LookAt(CameraController.Instance.transform);
                leftCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                leftCheckPoint.gameObject.SetActive(false);
            }
        
            if (_grapplingHookGears.hasAutoRightGrapplingHookCheckPoint)
            {
                rightCheckPoint.position = _grapplingHookGears.rightGrapplingHookCastHitHookCheckPoint;
                rightCheckPoint.LookAt(CameraController.Instance.transform);
                rightCheckPoint.gameObject.SetActive(true);
            }
            else
            {
                rightCheckPoint.gameObject.SetActive(false);
            }
        }
    }
}
