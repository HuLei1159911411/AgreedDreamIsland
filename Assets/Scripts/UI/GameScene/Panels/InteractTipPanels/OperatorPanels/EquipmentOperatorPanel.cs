using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentOperatorPanel : OperatorPanel
{
    public Color whiteLevelBackgroundColor;
    public Color blueLevelBackgroundColor;
    public Color purpleLevelBackgroundColor;
    public Color goldenLevelBackgroundColor;
    public Image equipmentIcon;
    
    private Equipment _nowEquipment;
    private float _nowLongPressTime;
    public override void ShowPanel()
    {
        base.ShowPanel();
        transform.gameObject.SetActive(true);
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
        interactiveObjectName.text = SetAndGetEquipmentStrName(_nowEquipment);
        
        switch (_nowEquipment.equipmentLevel)
        {
            case E_EquipmentLevel.White:
                backgroundImage.color = whiteLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Blue:
                backgroundImage.color = blueLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Purple:
                backgroundImage.color = purpleLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Golden:
                backgroundImage.color = goldenLevelBackgroundColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (_nowEquipment.equipmentName == E_EquipmentName.GrapplingHookGears)
        {
            equipmentIcon.sprite = ResourcesManager.Instance.LoadObject<Sprite>("WeaponIcon/GrapplingHookGears");
        }
        else
        {
            equipmentIcon.sprite = ResourcesManager.Instance.LoadObject<Sprite>("WeaponIcon/" +
                Enum.GetName(typeof(E_EquipmentName), _nowEquipment.equipmentName) + 
                "_" + 
                (_nowEquipment as Weapon).weaponModelIndex);
        }
        
        // 第一个操作(尝试拾取)
        if (EquipmentsController.Instance.CheckEquipmentIsFull(_nowEquipment.equipmentType))
        {
            listOperatorObjects[0].operatorTransform.gameObject.SetActive(true);
            
            listOperatorObjects[0].operatorIconTransform.gameObject.SetActive(true);
            listOperatorObjects[0].operatorIconImage.enabled = false;
            listOperatorObjects[0].operatorIconText.text = "已达最大装备数量";
            
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
        // 当前只能拾取摁下交互键(当前类型装备并不存在)
        if (EquipmentsController.Instance.CheckEquipmentIsEmpty(_nowEquipment.equipmentType) &&
            Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            return _nowEquipment.PickUpItem();
        }
        
        // 在可以拾取和替换的情况下摁下交互键
        if (EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType] != -1 &&
            Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            _nowLongPressTime += Time.deltaTime;
            listOperatorObjects[1].operatorIconImage.fillAmount =
                _nowLongPressTime < listOperatorObjects[1].longPressTime
                    ? _nowLongPressTime / listOperatorObjects[1].longPressTime
                    : 1f;
        }
        
        // 时间到了替换
        if (_nowLongPressTime >= listOperatorObjects[1].longPressTime &&
            EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType] != -1)
        {
            if (EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType] != -1 &&
                !EquipmentsController.Instance.nowEquipments[(int)_nowEquipment.equipmentType].isInUse)
            {
                EquipmentsController.Instance.ExchangeEquipment(_nowEquipment.equipmentType,
                    EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType], _nowEquipment);
            }
            else if(EquipmentsController.Instance.nowEquipmentsIndexes[(int)_nowEquipment.equipmentType] == -1)
            {
                EquipmentsController.Instance.ExchangeEquipment(_nowEquipment.equipmentType, 0, _nowEquipment);
                EquipmentsController.Instance.ChangeEquipment(_nowEquipment.equipmentType,0);
            }
            
            _nowLongPressTime = 0f;
            return true;
        }
        
        // 时间没到松开尝试捡起
        if (Input.GetKeyUp(InputManager.Instance.DicBehavior[E_InputBehavior.Interact]))
        {
            listOperatorObjects[1].operatorIconImage.fillAmount = 1f;
            _nowLongPressTime = 0f;
            return _nowEquipment.PickUpItem();
        }

        return false;
    }

    private string SetAndGetEquipmentStrName(Equipment equipment)
    {
        switch (equipment.equipmentName)
        {
            case E_EquipmentName.GrapplingHookGears:
                equipment.strName = "立体机动装置";
                return equipment.strName;
            case E_EquipmentName.Rod:
                switch ((equipment as Weapon).weaponModelIndex)
                {
                    case 0 :
                        equipment.strName = "棍";
                        break;
                    case 1:
                        equipment.strName = "蛇杖";
                        break;
                    case 2:
                        equipment.strName = "干树枝";
                        break;
                    case 3:
                        equipment.strName = "权杖";
                        break;
                    case 4:
                        equipment.strName = "树干桩";
                        break;
                }
                
                return equipment.strName;
            case E_EquipmentName.Sword:
                switch ((equipment as Weapon).weaponModelIndex)
                {
                    case 0 :
                        equipment.strName =  "刀";
                        break;
                    case 1:
                        equipment.strName =  "木棒";
                        break;
                    case 2:
                        equipment.strName =  "砍刀";
                        break;
                    case 3:
                        equipment.strName =  "花果棒";
                        break;
                    case 4:
                        equipment.strName =  "水管";
                        break;
                    case 5 :
                        equipment.strName =  "砖锤";
                        break;
                    case 6 :
                        equipment.strName =  "长刀";
                        break;
                }
                
                return equipment.strName;
            case E_EquipmentName.Sickle:
                switch ((equipment as Weapon).weaponModelIndex)
                {
                    case 0 :
                        equipment.strName =  "镰刀";
                        break;
                    case 1:
                        equipment.strName =  "锤子";
                        break;
                    case 2:
                        equipment.strName =  "双面斧";
                        break;
                    case 3:
                        equipment.strName =  "弯刀";
                        break;
                }
                
                return equipment.strName;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
