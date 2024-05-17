using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraController : MonoBehaviour
{
    private static UICameraController _instance;
    public static UICameraController Instance => _instance;

    public Camera uiCamera;
    // 临时保存在世界坐标下从MainCamera到目标点的方向向量
    private Vector3 _relativeDirection;
    // 临时保存在世界坐标下从MainCamera到目标点的距离
    private float _distance;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        uiCamera = GetComponent<Camera>();
    }

    public Vector3 GetRelativeMainCameraToUICameraPosition(Vector3 position)
    {
        _relativeDirection = position - CameraController.Instance.transform.position;
        _distance = _relativeDirection.magnitude;
        _relativeDirection = transform.InverseTransformDirection(CameraController.Instance.transform.InverseTransformDirection(_relativeDirection)
                .normalized).normalized;
        return transform.position + _distance * _relativeDirection;
    }
}
