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
    // 格装备最大数量
    public List<int> listEquipmentTypesMaxCounts;
    // 所有的当前装备
    public Equipment[] nowEquipments;
    // 所有的当前装备在其装备栏中的索引值
    public int[] nowEquipmentsIndexes;
    
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

        nowEquipments = new Equipment[Enum.GetValues(typeof(E_EquipmentType)).Length];
        nowEquipmentsIndexes = new int[Enum.GetValues(typeof(E_EquipmentType)).Length];
        for (_count = 0; _count < nowEquipmentsIndexes.Length; _count++)
        {
            nowEquipmentsIndexes[_count] = -1;
        }
        
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
        if (nowEquipmentsIndexes[(int)E_EquipmentType.MovementEquipment] > -1)
        {
            nowEquipments[(int)E_EquipmentType.MovementEquipment].ListenEquipmentUse();
        }
    }
    
    // 监听装备使用输入
    public void UpdateEquipmentUseInputInformation()
    {
        equipmentUseInputInfo.HookShootLeftInput =
            Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootLeft]);
        equipmentUseInputInfo.HookShootRightInput =
            Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootRight]);
        equipmentUseInputInfo.FireInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Fire]);
        equipmentUseInputInfo.AimInput = Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.Aim]);
    }

    // 添加装备
    public bool AddEquipment(Equipment equipment)
    {
        // 判断是否这种装备是否装满了
        if (!CheckEquipmentIsFull(equipment.equipmentType))
        {
            listEquipments[(int)equipment.equipmentType].Add(equipment);
            
            // 在当前类型装备还没有装备时，添加装备后自动装备该装备
            if (nowEquipmentsIndexes[(int)equipment.equipmentType] == -1)
            {
                ChangeEquipment(equipment.equipmentType, listEquipments.Count - 1);
            }

            return true;
        }
        else
        {
            return false;
        }
    }
    
    // 扔下装备
    public void DiscardEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        // 当前装备正在装备中
        if (nowEquipmentsIndexes[(int)equipmentType] == equipmentIndex)
        {
            RemoveEquipment(equipmentType);
        }
        listEquipments[(int)equipmentType][equipmentIndex].DiscardItem();
        listEquipments[(int)equipmentType][equipmentIndex].controller = null;
        listEquipments[(int)equipmentType].RemoveAt(equipmentIndex);
    }
    
    // 卸下装备(取消装备)
    public bool RemoveEquipment(E_EquipmentType equipmentType)
    {
        nowEquipments[(int)equipmentType].RemoveEquipment();
                
        nowEquipments[(int)equipmentType] = null;
        nowEquipmentsIndexes[(int)equipmentType] = -1;
        return true;
    }
    
    // 在已有装备中切换装备
    public void ChangeEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        if (equipmentIndex == -1)
        {
            return;
        }
        
        // 当前装备了装备进行切换装备先将当前装备卸下
        if (nowEquipmentsIndexes[(int)equipmentType] != -1)
        {
            RemoveEquipment(equipmentType);
        }

        nowEquipments[(int)equipmentType] = listEquipments[(int)equipmentType][equipmentIndex];
        nowEquipments[(int)equipmentType].WearEquipment();
        nowEquipmentsIndexes[(int)equipmentType] = equipmentIndex;
        
    }
    
    // 判断该类型装备是否已满
    public bool CheckEquipmentIsFull(E_EquipmentType equipmentType)
    {
        return listEquipments[(int)equipmentType].Count == listEquipmentTypesMaxCounts[(int)equipmentType];
    }
}
