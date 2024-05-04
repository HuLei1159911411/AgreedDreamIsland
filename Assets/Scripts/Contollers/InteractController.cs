using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractController : MonoBehaviour
{
    private static InteractController _instance;
    public static InteractController Instance => _instance;
    
    public Collider interactCollider;
    public List<string> interactiveObjectsTags;
    public InteractPanel interactPanel;

    private int _count;
    // 当前可交互游戏对象碰撞盒
    public Collider nowInteractiveObjectCollider;
    // 当前可交互对象
    private InteractiveObject _nowInteractiveObject;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
    }

    void Update()
    {
        bool isNotNull = nowInteractiveObjectCollider is not null;
        if (nowInteractiveObjectCollider != null)
        {
            if (_nowInteractiveObject == null && nowInteractiveObjectCollider != null)
            {
                _nowInteractiveObject = nowInteractiveObjectCollider.transform.GetComponent<InteractiveObject>();
            }

            if (!(interactPanel is null))
            {
                if (!interactPanel.isShow)
                {
                    // 设置UI页面
                    interactPanel.SetInteractInteractPanel(_nowInteractiveObject);
                    interactPanel.ShowPanel();
                }

                // 监听交互界面交互结果
                if (interactPanel.ListenInteract())
                {
                    nowInteractiveObjectCollider.enabled = false;
                    ClearNowInteractiveObject();
                }
            }
        }
        else
        {
            if (interactPanel is not null && interactPanel.isShow)
            {
                interactPanel.ClosePanel();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (nowInteractiveObjectCollider == null)
        {
            for (_count = 0; _count < interactiveObjectsTags.Count; _count++)
            {
                if (other.CompareTag(interactiveObjectsTags[_count]))
                {
                    nowInteractiveObjectCollider = other;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == nowInteractiveObjectCollider)
        {
            ClearNowInteractiveObject();
        }
    }

    private void ClearNowInteractiveObject()
    {
        interactPanel.ClosePanel();
        _nowInteractiveObject = null;
        nowInteractiveObjectCollider = null;
    }
}
