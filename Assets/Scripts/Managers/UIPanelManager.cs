using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class UIPanelManager : MonoBehaviour
{
    private static UIPanelManager _instance;
    public static UIPanelManager Instance => _instance;
    
    public event UnityAction PanelsInputEvent;

    public List<Panel> listPanels;

    private int _count;
    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        listPanels = new List<Panel>();
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
