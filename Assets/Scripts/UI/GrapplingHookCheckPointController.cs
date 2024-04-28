using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHookCheckPointController : MonoBehaviour
{
    public Transform midCheckPoint;
    public Transform leftCheckPoint;
    public Transform rightCheckPoint;
    private void Update()
    {
        if (PlayerMovementStateMachine.Instance.hasHookCheckPoint)
        {
            midCheckPoint.position = PlayerMovementStateMachine.Instance.hookCheckPoint;
            midCheckPoint.LookAt(CameraController.Instance.transform);
            midCheckPoint.gameObject.SetActive(true);
        }
        else
        {
            midCheckPoint.gameObject.SetActive(false);
        }
        
        if (PlayerMovementStateMachine.Instance.hasAutoLeftGrapplingHookCheckPoint)
        {
            leftCheckPoint.position = PlayerMovementStateMachine.Instance.leftGrapplingHookCastHitHookCheckPoint;
            leftCheckPoint.LookAt(CameraController.Instance.transform);
            leftCheckPoint.gameObject.SetActive(true);
        }
        else
        {
            leftCheckPoint.gameObject.SetActive(false);
        }
        
        if (PlayerMovementStateMachine.Instance.hasAutoRightGrapplingHookCheckPoint)
        {
            rightCheckPoint.position = PlayerMovementStateMachine.Instance.rightGrapplingHookCastHitHookCheckPoint;
            rightCheckPoint.LookAt(CameraController.Instance.transform);
            rightCheckPoint.gameObject.SetActive(true);
        }
        else
        {
            rightCheckPoint.gameObject.SetActive(false);
        }
    }
}
