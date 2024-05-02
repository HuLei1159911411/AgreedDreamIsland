using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_EquipmentType
{
    MovementEquipment,
}

public struct EquipmentUseInputInformation
{
    public bool HookShootLeftInput;
    public bool HookShootRightInput;
    public bool FireInput;
    public bool AimInput;
}

public class EquipmentsController : MonoBehaviour
{
    private static EquipmentsController _instance;
    public static EquipmentsController Instance => _instance;
    
    // 玩家行为树状态机
    [HideInInspector] public PlayerMovementStateMachine playerMovementStateMachine;
    // 玩家Transform
    [HideInInspector] public Transform playerTransform;
    // 玩家脚部Transform
    [HideInInspector] public Transform playerFootTransform;
    // 玩家钩锁装备父节点位置
    public Transform grapplingHookGearsFatherTransform;
    // 装备控制输入信息
    public EquipmentUseInputInformation equipmentUseInputInfo;
    
    // 装备栏
    public List<List<Equipment>> listEquipments;
    
    // 移动装备栏大小
    public int movementEquipmentMaxCount = 1;
    // 当前的移动装备
    public Equipment nowMovementEquipment;
    // 当前移动装备的在所有移动装备中的索引值(-1为未进行装备)
    public int nowMovementEquipmentIndex;
    
    // 计数用临时变量
    private int _count;
    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        listEquipments = new List<List<Equipment>>()
        {
            { new List<Equipment>() }
        };
        
        nowMovementEquipmentIndex = -1;
        playerTransform = transform;
    }

    private void Start()
    {
        playerMovementStateMachine = PlayerMovementStateMachine.Instance;
        playerFootTransform = playerMovementStateMachine.footRaycastEmissionTransform;
        playerMovementStateMachine.WhenUpdateLast += ListenEquipmentsUse;
    }

    // 监听装备使用
    public void ListenEquipmentsUse()
    {
        UpdateEquipmentUseInputInformation();
        if (nowMovementEquipmentIndex > -1)
        {
            nowMovementEquipment.ListenEquipmentUse();
        }
    }
    
    // 监听装备使用输入
    public void UpdateEquipmentUseInputInformation()
    {
        equipmentUseInputInfo.HookShootLeftInput =
            Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootLeft]);
        equipmentUseInputInfo.HookShootRightInput =
            Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootRight]);
        equipmentUseInputInfo.FireInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Fire]);
        equipmentUseInputInfo.AimInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Aim]);
    }

    // 添加装备
    public bool AddEquipment(Equipment equipment)
    {
        switch (equipment.equipmentType)
        {
            case E_EquipmentType.MovementEquipment:
                if (listEquipments[(int)equipment.equipmentType].Count < movementEquipmentMaxCount)
                {
                    listEquipments[(int)E_EquipmentType.MovementEquipment].Add(equipment);
                    // 在当前类型装备还没有装备时，添加装备自动装备该装备
                    if (nowMovementEquipmentIndex == -1)
                    {
                        ChangeEquipment(equipment.equipmentType, listEquipments.Count - 1);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 扔下装备
    public void DiscardEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        switch (equipmentType)
        {
            case E_EquipmentType.MovementEquipment:
                RemoveEquipment(equipmentType, equipmentIndex);
                listEquipments[(int)equipmentType][equipmentIndex].DiscardItem();
                listEquipments[(int)equipmentType][equipmentIndex].controller = null;
                listEquipments[(int)equipmentType].RemoveAt(equipmentIndex);
                
                ChangeEquipment(equipmentType, listEquipments[(int)equipmentType].Count - 1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(equipmentType), equipmentType, null);
        }
    }
    
    // 卸下装备
    public bool RemoveEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        switch (equipmentType)
        {
            case E_EquipmentType.MovementEquipment:
                listEquipments[(int)equipmentType][equipmentIndex].RemoveEquipment();
                nowMovementEquipmentIndex = -1;
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // 切换装备
    public void ChangeEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        if (equipmentIndex == -1)
        {
            return;
        }
        
        switch (equipmentType)
        {
            case E_EquipmentType.MovementEquipment:
                
                // 当前装备了装备进行切换装备先将当前装备卸下
                if (nowMovementEquipmentIndex != -1)
                {
                    nowMovementEquipment.RemoveEquipment();
                }
                
                nowMovementEquipment = listEquipments[(int)equipmentType][equipmentIndex];
                nowMovementEquipment.WearEquipment();
                nowMovementEquipmentIndex = equipmentIndex;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(equipmentType), equipmentType, null);
        }
    }
}
