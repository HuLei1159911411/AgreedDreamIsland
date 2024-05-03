using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InteractPanel : Panel
{
    public OperatorPanel nowOperatorPanel;

    public EquipmentOperatorPanel equipmentOperatorPanel;

    public void Start()
    {
        InteractController.Instance.interactPanel = this;
        ClosePanel();
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        gameObject.SetActive(true);
        nowOperatorPanel.ShowPanel();
        
        UIPanelManager.Instance.listPanels.Add(this);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        gameObject.SetActive(false);
        if (!(nowOperatorPanel is null))
        {
            nowOperatorPanel.ClosePanel();
        }
        
        UIPanelManager.Instance.listPanels.Remove(this);
    }
    
    public void SetInteractInteractPanel(InteractiveObject interactiveObject)
    {
        // 操作UI选择
        switch (interactiveObject.type)
        {
            case E_InteractiveObjectType.Item:
                switch ((interactiveObject as Item).itemType)
                {
                    case E_ItemType.Equipment:
                        nowOperatorPanel = equipmentOperatorPanel;
                        break;
                    case E_ItemType.Supply:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        nowOperatorPanel.UpdateOperatorInformation(interactiveObject);
    }
    
    // 监听输入
    public bool ListenInteract()
    {
        return nowOperatorPanel.ListenInteractOperators();
    }
}
