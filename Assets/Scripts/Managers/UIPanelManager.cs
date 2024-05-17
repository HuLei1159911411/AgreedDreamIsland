using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    private static UIPanelManager _instance;
    public static UIPanelManager Instance => _instance;

    public E_Theme theme;
    public event UnityAction PanelsInputEvent;
    public List<Panel> listPanels;
    public Transform lowLayer;
    public RectTransform canvasRectTransform;
    public GraphicRaycaster graphicRaycaster;
    
    private int _count;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        listPanels = new List<Panel>();
    }

    private void Start()
    {
        graphicRaycaster = canvasRectTransform.GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (!(InputManager.Instance is null))
        {
            PanelsInputEvent?.Invoke();
        }
    }

    public void CloseAllPanels()
    {
        for (_count = 0; _count < listPanels.Count; _count++)
        {
            if (listPanels[_count].isShow)
            {
                listPanels[_count].ClosePanel();
            }
        }
    }
    
}
