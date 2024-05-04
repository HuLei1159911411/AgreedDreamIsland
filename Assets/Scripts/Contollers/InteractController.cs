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
    public InteractTipPanel interactTipPanel;

    private int _count;
    // 当前可交互游戏对象碰撞盒
    public Collider nowInteractiveObjectCollider;
    // 当前可交互对象
    private InteractiveObject _nowInteractiveObject;
    // 拾取冷却计时器
    private float _coolDownTimer;

    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        _coolDownTimer = 0f;
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

            if (!(interactTipPanel is null))
            {
                if (!interactTipPanel.isShow)
                {
                    // 设置UI页面
                    interactTipPanel.SetInteractInteractPanel(_nowInteractiveObject);
                    interactTipPanel.ShowPanel();
                }

                // 监听交互界面交互结果
                if (interactTipPanel.ListenInteract())
                {
                    nowInteractiveObjectCollider.enabled = false;
                    ClearNowInteractiveObject();
                }
            }
        }
        else
        {
            if (interactTipPanel is not null && interactTipPanel.isShow)
            {
                interactTipPanel.ClosePanel();
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

    private void OnTriggerStay(Collider other)
    {
        if (_nowInteractiveObject == null)
        {
            _coolDownTimer += Time.fixedTime;
            if (_coolDownTimer > 0.1f)
            {
                for (_count = 0; _count < interactiveObjectsTags.Count; _count++)
                {
                    if (other.CompareTag(interactiveObjectsTags[_count]))
                    {
                        nowInteractiveObjectCollider = other;
                        _coolDownTimer = 0f;
                    }
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
        interactTipPanel.ClosePanel();
        _nowInteractiveObject = null;
        nowInteractiveObjectCollider = null;
    }
}
