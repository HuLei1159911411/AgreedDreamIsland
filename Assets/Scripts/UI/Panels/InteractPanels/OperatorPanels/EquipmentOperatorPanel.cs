using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentOperatorPanel : OperatorPanel
{
    private Equipment _nowEquipment;
    private float _nowLongPressTime;
    public override void ShowPanel()
    {
        base.ShowPanel();
        this.gameObject.SetActive(true);
        interactiveObjectName.gameObject.SetActive(true);
        backgroundImage.gameObject.SetActive(true);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        this.gameObject.SetActive(false);
    }

    public override void UpdateOperatorInformation(InteractiveObject interactiveObject)
    {
        _nowEquipment = interactiveObject as Equipment;
        interactiveObjectName.text = interactiveObject.strName;
        
        // 第一个操作(尝试拾取)
        if (EquipmentsController.Instance.CheckEquipmentIsFull(_nowEquipment.equipmentType))
        {
            listOperatorObjects[0].operatorTransform.gameObject.SetActive(true);
            
            listOperatorObjects[0].operatorIconTransform.gameObject.SetActive(true);
            listOperatorObjects[0].operatorIconImage.enabled = false;
            listOperatorObjects[0].operatorIconText.text = "已达到最大装备数量";
            
            listOperatorObjects[0].operatorText.enabled = false;
        }
        else
        {
            listOperatorObjects[0].operatorTransform.gameObject.SetActive(true);
            
            listOperatorObjects[0].operatorIconTransform.gameObject.SetActive(true);
            listOperatorObjects[0].operatorIconImage.enabled = true;
            listOperatorObjects[0].operatorIconText.text = Enum.GetName(typeof(KeyCode),
                InputManager.Instance.DicBehavior[E_InputBehavior.Interact]);
            listOperatorObjects[0].operatorText.text = "拾取";
            
            listOperatorObjects[0].operatorText.enabled = true;
        }
        
        // 第二个操作(尝试替换)
        // 当前装备了这种类型的装备
        if (EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType] != -1)
        {
            listOperatorObjects[1].operatorTransform.gameObject.SetActive(true);
            
            listOperatorObjects[1].operatorIconTransform.gameObject.SetActive(true);
            listOperatorObjects[1].operatorIconImage.enabled = true;
            listOperatorObjects[1].operatorIconImage.fillAmount = 1f;
            listOperatorObjects[1].operatorIconText.text = Enum.GetName(typeof(KeyCode),
                InputManager.Instance.DicBehavior[E_InputBehavior.Interact]);
            listOperatorObjects[1].operatorText.text = "(长摁)替换";
            
            listOperatorObjects[1].operatorText.enabled = true;

            _nowLongPressTime = 0f;
        }
        // 当前并未装备这种类型的装备
        else
        {
            listOperatorObjects[1].operatorTransform.gameObject.SetActive(false);
        }
    }

    public override bool ListenInteractOperators()
    {
        // 当前只能拾取摁下交互键
        if (!EquipmentsController.Instance.CheckEquipmentIsFull(_nowEquipment.equipmentType) &&
            Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            return _nowEquipment.PickUpItem();
        }
        
        // 在可以拾取和替换的情况下摁下交互键
        if (Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            _nowLongPressTime += Time.deltaTime;
            listOperatorObjects[1].operatorIconImage.fillAmount =
                _nowLongPressTime < listOperatorObjects[1].longPressTime
                    ? _nowLongPressTime / listOperatorObjects[1].longPressTime
                    : 1f;
        }
        
        // 时间到了替换
        if (_nowLongPressTime >= listOperatorObjects[1].longPressTime)
        {
            EquipmentsController.Instance.DiscardEquipment(_nowEquipment.equipmentType,
                EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType]);
            _nowEquipment.PickUpItem();
            _nowLongPressTime = 0f;
            return true;
        }
        
        // 时间没到松开尝试
        if (Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            listOperatorObjects[1].operatorIconImage.fillAmount = 1f;
            _nowLongPressTime = 0f;
            return _nowEquipment.PickUpItem();
        }

        return false;
    }
}
