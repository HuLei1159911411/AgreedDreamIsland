using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_EquipmentType
{
    Weapon,
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
    
    // UI
    public WeaponsBagPanel weaponsBagPanel;
    
    // 计数用临时变量
    private int _count;
    // 交换装备临时用变量
    private Equipment _tempEquipment;
    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        
        listEquipments = new List<List<Equipment>>();
        nowEquipments = new Equipment[Enum.GetValues(typeof(E_EquipmentType)).Length];
        nowEquipmentsIndexes = new int[Enum.GetValues(typeof(E_EquipmentType)).Length];
        for (_count = 0; _count < nowEquipmentsIndexes.Length; _count++)
        {
            nowEquipmentsIndexes[_count] = -1;
            listEquipments.Add(new List<Equipment>());
        }
        
        playerTransform = transform;
    }

    private void Start()
    {
        playerMovementStateMachine = PlayerMovementStateMachine.Instance;
        playerFootTransform = playerMovementStateMachine.footRaycastEmissionTransform;
        // 在状态机Update结尾执行事件中注册监听装备使用的委托
        playerMovementStateMachine.WhenUpdateLast += ListenEquipmentsUse;
    }

    // 监听装备使用
    public void ListenEquipmentsUse()
    {
        UpdateEquipmentUseInputInformation();

        for (_count = 0; _count < nowEquipmentsIndexes.Length; _count++)
        {
            if (nowEquipmentsIndexes[_count] != -1)
            {
                nowEquipments[_count].ListenEquipmentUse();
            }
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
            // 对武器进行特殊处理，优先将武器放入前两格中
            if (equipment.equipmentType == E_EquipmentType.Weapon && 
                listEquipments[(int)E_EquipmentType.Weapon].Count != 0)
            {
                if (listEquipments[(int)E_EquipmentType.Weapon][0] == null)
                {
                    listEquipments[(int)E_EquipmentType.Weapon][0] = equipment;
                    // 在当前类型装备还没有装备时，添加装备后自动装备该装备
                    if (nowEquipmentsIndexes[(int)equipment.equipmentType] == -1)
                    {
                        ChangeEquipment(equipment.equipmentType, 0);
                    }

                    return true;
                }
                else if (listEquipments[(int)E_EquipmentType.Weapon].Count >= 2 && listEquipments[(int)E_EquipmentType.Weapon][1] == null)
                {
                    listEquipments[(int)E_EquipmentType.Weapon][1] = equipment;
                    // 在当前类型装备还没有装备时，添加装备后自动装备该装备
                    if (nowEquipmentsIndexes[(int)equipment.equipmentType] == -1)
                    {
                        ChangeEquipment(equipment.equipmentType, 1);
                    }

                    return true;
                }
            }

            listEquipments[(int)equipment.equipmentType].Add(equipment);
            // 在当前类型装备还没有装备时，添加装备后自动装备该装备
            if (nowEquipmentsIndexes[(int)equipment.equipmentType] == -1)
            {
                ChangeEquipment(equipment.equipmentType, listEquipments.Count - 1);
            }
            
            if (equipment.equipmentType == E_EquipmentType.Weapon)
            {
                weaponsBagPanel.SetWeaponsBagByEquipmentsController();
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
        
        // 如果是前两格的扔下
        if (equipmentIndex < 2 && equipmentType == E_EquipmentType.Weapon)
        {
            listEquipments[(int)equipmentType][equipmentIndex] = null;
            if (equipmentType == E_EquipmentType.Weapon)
            {
                weaponsBagPanel.SetNowWeaponCell(-1);
            }
        }
        else
        {
            listEquipments[(int)equipmentType].RemoveAt(equipmentIndex);
        }
    }
    
    // 卸下装备(取消装备)
    public bool RemoveEquipment(E_EquipmentType equipmentType)
    {
        nowEquipments[(int)equipmentType].RemoveEquipment();
        
        nowEquipments[(int)equipmentType] = null;
        nowEquipmentsIndexes[(int)equipmentType] = -1;

        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetNowWeaponCell(-1);
        }
        
        return true;
    }
    
    // 在已有装备中切换装备
    public void ChangeEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        
        if (equipmentIndex == -1)
        {
            return;
        }
        
        // 当前装备了装备进行切换装备先将当前装备卸下(并且交换装备)
        if (nowEquipmentsIndexes[(int)equipmentType] != -1)
        {
            nowEquipments[(int)equipmentType].RemoveEquipment();
            
            _tempEquipment = listEquipments[(int)equipmentType][nowEquipmentsIndexes[(int)equipmentType]];
            listEquipments[(int)equipmentType][nowEquipmentsIndexes[(int)equipmentType]] =
                listEquipments[(int)equipmentType][equipmentIndex];
            nowEquipments[(int)equipmentType] = listEquipments[(int)equipmentType][equipmentIndex];
            listEquipments[(int)equipmentType][equipmentIndex] = _tempEquipment;
            nowEquipments[(int)equipmentType].WearEquipment();
        }
        else
        {
            nowEquipments[(int)equipmentType] = listEquipments[(int)equipmentType][equipmentIndex];
            nowEquipments[(int)equipmentType].WearEquipment();
            nowEquipmentsIndexes[(int)equipmentType] = equipmentIndex;
        }
        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetWeaponsBagByEquipmentsController();
        }
    }
    
    // 判断该类型装备是否已满
    public bool CheckEquipmentIsFull(E_EquipmentType equipmentType)
    {
        if (equipmentType == E_EquipmentType.Weapon)
        {
            return listEquipments[(int)equipmentType].Count == listEquipmentTypesMaxCounts[(int)equipmentType] &&
                   listEquipments[(int)equipmentType][0] != null && listEquipments[(int)equipmentType][1] != null;
        }
        
        return listEquipments[(int)equipmentType].Count == listEquipmentTypesMaxCounts[(int)equipmentType];
    }
}
