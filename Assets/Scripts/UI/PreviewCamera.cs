using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class PreviewCamera : MonoBehaviour
{
    public CameraController MainCamera;


    void OnGUI()
    {
        if (GUI.Button(new Rect(200, 30, 120, 50), "预览第一人称"))
        {
            PreviewCameraPerspective(E_CameraView.FirstPerson);
        }

        if (GUI.Button(new Rect(350, 30, 120, 50), "预览第三人称"))
        {
            PreviewCameraPerspective(E_CameraView.ThirdPerson);
        }

        if (GUI.Button(new Rect(500, 30, 150, 50), "预览更远的第三人称"))
        {
            PreviewCameraPerspective(E_CameraView.ThirdPersonFurther);
        }
    }

   /* 
#if UNITY_EDITOR
    // 若需保证Editor下一直调用Update、OnGUI这些函数加入下面的函数，强制在OnDrawGizmos()中调用场景刷新的API，不需要的时候注释OnDrawGizmos()
    void OnDrawGizmos()
    {
        // Your gizmo drawing thing goes here if required...
        // Ensure continuous Update calls.
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
    }
#endif
    */

    void PreviewCameraPerspective(E_CameraView view)
    {
        if (!(MainCamera is null))
        {
            MainCamera.ChangeCameraView(view);
            MainCamera.UpdateCameraPositionAndRotationImmediately();
        }
    }
}