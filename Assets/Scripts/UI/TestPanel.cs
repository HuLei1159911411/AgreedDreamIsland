using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel : MonoBehaviour
{
    public Text NowView;
    public CameraController mainCamera;

    private Dictionary<E_CameraView, string> _dicCameraView = new Dictionary<E_CameraView, string>();
    private E_CameraView _cameraView;
    private void Start()
    {
        _cameraView = mainCamera.nowView;
        _dicCameraView.Add(E_CameraView.FirstPerson,"第一人称");
        _dicCameraView.Add(E_CameraView.ThirdPerson,"第三人称");
        _dicCameraView.Add(E_CameraView.ThirdPersonFurther,"更远的第三人称");
        NowView.text = _dicCameraView[_cameraView];
    }

    void Update()
    {
        if (_cameraView != mainCamera.nowView)
        {
            _cameraView = mainCamera.nowView;
            NowView.text = _dicCameraView[_cameraView];
        }
    }
}
