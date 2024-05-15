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
    // 玩家动画状态机
    [HideInInspector] public Animator playerAnimator;
    // 玩家角色管理
    [HideInInspector] public PlayerCharacter playerCharacter;
    // 玩家Transform
    [HideInInspector] public Transform playerTransform;
    // 玩家脚部Transform
    [HideInInspector] public Transform playerFootTransform;
    // 玩家钩锁装备父节点位置
    public Transform grapplingHookGearsFatherTransform;
    // 玩家武器装备父节点位置
    public Transform weaponOnRightHandFatherTransform;
    // 玩家武器未装备父节点位置
    public Transform weaponOnBackFatherTransform;
    
    // 装备控制输入信息
    public EquipmentUseInputInformation EquipmentUseInputInfo;
    
    // 装备栏
    public List<Equipment[]> ListEquipments;
    // 格装备最大数量
    public List<int> listEquipmentTypesMaxCounts;
    // 所有的当前装备
    public Equipment[] nowEquipments;
    // 所有的当前装备在其装备栏中的索引值
    public int[] nowEquipmentsIndexes;
    // 当前不同装备的装备数量
    public int[] nowEquipmentCount;
    
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
        
        ListEquipments = new List<Equipment[]>();
        nowEquipments = new Equipment[Enum.GetValues(typeof(E_EquipmentType)).Length];
        nowEquipmentsIndexes = new int[Enum.GetValues(typeof(E_EquipmentType)).Length];
        nowEquipmentCount = new int[Enum.GetValues(typeof(E_EquipmentType)).Length];
        for (_count = 0; _count < nowEquipmentsIndexes.Length; _count++)
        {
            nowEquipmentsIndexes[_count] = -1;
            ListEquipments.Add(new Equipment[listEquipmentTypesMaxCounts[_count]]);
        }
        
        playerTransform = transform;
        playerCharacter = GetComponent<PlayerCharacter>();
    }

    private void Start()
    {
        playerMovementStateMachine = PlayerMovementStateMachine.Instance;
        playerAnimator = playerMovementStateMachine.playerAnimator;
        playerFootTransform = playerMovementStateMachine.footRaycastEmissionTransform;
        // 在状态机Update结尾执行事件中注册监听装备使用的委托
        playerMovementStateMachine.WhenUpdateLast += ListenEquipmentsUse;
    }

    // 监听装备使用
    public void ListenEquipmentsUse()
    {
        // 监听装备切换
        if (ListenEquipmentsChange())
        {
            return;
        }
        
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
        EquipmentUseInputInfo.HookShootLeftInput =
            Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootLeft]);
        EquipmentUseInputInfo.HookShootRightInput =
            Input.GetKey(InputManager.Instance.DicBehavior[E_InputBehavior.HookShootRight]);
        EquipmentUseInputInfo.FireInput = Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Fire]);
        EquipmentUseInputInfo.AimInput = Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.Aim]);
    }

    // 添加装备
    public bool AddEquipment(Equipment equipment)
    {
        // 判断是否这种装备是否装满了
        if (!CheckEquipmentIsFull(equipment.equipmentType))
        {
            // 找到最小空位置放入装备
            for (_count = 0; _count < ListEquipments[(int)equipment.equipmentType].Length; _count++)
            {
                if (ListEquipments[(int)equipment.equipmentType][_count] == null)
                {
                    ListEquipments[(int)equipment.equipmentType][_count] = equipment;
                    nowEquipmentCount[(int)equipment.equipmentType]++;
                    if (equipment.equipmentType == E_EquipmentType.Weapon)
                    {
                        weaponsBagPanel.SetWeaponsBagByEquipmentsController();
                    }
                    return true;
                }
            }

            return false;
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
        
        ListEquipments[(int)equipmentType][equipmentIndex].DiscardItem(playerTransform.position);
        ListEquipments[(int)equipmentType][equipmentIndex].controller = null;

        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetWeaponsBagByEquipmentsController();
        }

        ListEquipments[(int)equipmentType][equipmentIndex] = null;
        nowEquipmentCount[(int)equipmentType]--;
    }
    
    // 卸下装备(取消装备)
    public bool RemoveEquipment(E_EquipmentType equipmentType)
    {
        nowEquipments[(int)equipmentType].RemoveEquipment();
        
        nowEquipments[(int)equipmentType] = null;
        nowEquipmentsIndexes[(int)equipmentType] = -1;

        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetNowEquippedWeaponCell(-1);
        }
        
        return true;
    }
    
    // 在已有装备中切换装备
    public void ChangeEquipment(E_EquipmentType equipmentType, int equipmentIndex)
    {
        if (equipmentIndex == nowEquipmentsIndexes[(int)equipmentType] ||
            equipmentIndex == -1 ||
            ListEquipments[(int)equipmentType] == null)
        {
            return;
        }

        if (nowEquipmentsIndexes[(int)equipmentType] != -1)
        {
            nowEquipments[(int)equipmentType].RemoveEquipment();
        }
        
        nowEquipments[(int)equipmentType] = ListEquipments[(int)equipmentType][equipmentIndex];
        nowEquipmentsIndexes[(int)equipmentType] = equipmentIndex;
        nowEquipments[(int)equipmentType].WearEquipment();
        
        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetWeaponsBagByEquipmentsController();
        }
    }
    
    // 交换装备
    public bool ExchangeEquipment(E_EquipmentType equipmentType, int index1, int index2)
    {
        // 当前装备了装备进行切换装备先将当前装备卸下(并且交换装备)
        if (nowEquipmentsIndexes[(int)equipmentType] != -1 && 
            (nowEquipmentsIndexes[(int)equipmentType] == index1 ||
             nowEquipmentsIndexes[(int)equipmentType] == index2))
        {
            nowEquipments[(int)equipmentType].RemoveEquipment();
            
            _tempEquipment = ListEquipments[(int)equipmentType][index1];
            ListEquipments[(int)equipmentType][index1] =
                ListEquipments[(int)equipmentType][index2];
            ListEquipments[(int)equipmentType][index2] = _tempEquipment;
            
            nowEquipments[(int)equipmentType] = ListEquipments[(int)equipmentType][index2];
            nowEquipments[(int)equipmentType].WearEquipment();
        }
        else
        {
            _tempEquipment = ListEquipments[(int)equipmentType][index1];
            ListEquipments[(int)equipmentType][index1] =
                ListEquipments[(int)equipmentType][index2];
            ListEquipments[(int)equipmentType][index2] = _tempEquipment;
        }

        if (equipmentType == E_EquipmentType.Weapon)
        {
            weaponsBagPanel.SetWeaponsBagByEquipmentsController();
        }

        return true;
    }
    // 交换装备
    public bool ExchangeEquipment(E_EquipmentType equipmentType, int index, Equipment equipment)
    {
        if (CheckEquipmentIsFull(equipmentType))
        {
            if (nowEquipmentsIndexes[(int)equipmentType] == index)
            {
                DiscardEquipment(equipmentType, index);
                equipment.PickUpItem();
                ChangeEquipment(equipmentType, index);
            }
            else
            {
                DiscardEquipment(equipmentType, index);
                equipment.PickUpItem();
                ChangeEquipment(equipmentType, index);
            }

            return true;
        }
        
        // 找到最小空位置放入装备
        for (_count = 0; _count < ListEquipments[(int)equipment.equipmentType].Length; _count++)
        {
            if (ListEquipments[(int)equipment.equipmentType][_count] == null)
            {
                ListEquipments[(int)equipment.equipmentType][_count] = equipment;
                ExchangeEquipment(equipmentType, index, _count);
                return true;
            }
        }

        return false;
    }
    
    // 判断该类型装备是否已满
    public bool CheckEquipmentIsFull(E_EquipmentType equipmentType)
    {
        return nowEquipmentCount[(int)equipmentType] == listEquipmentTypesMaxCounts[(int)equipmentType];
    }
    
    // 判断该类型装备数量是否为0
    public bool CheckEquipmentIsEmpty(E_EquipmentType equipmentType)
    {
        return nowEquipmentCount[(int)equipmentType] == 0;
    }
    
    // 监听装备切换
    public bool ListenEquipmentsChange()
    {
        // 当前并没有装备装备
        if (nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] == -1)
        {
            if (ListEquipments[(int)E_EquipmentType.Weapon][0] != null &&
                Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.FirstWeapon]))
            {
                ChangeEquipment(E_EquipmentType.Weapon, 0);
                return true;
            }
            
            if (ListEquipments[(int)E_EquipmentType.Weapon][1] != null &&
                Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.SecondWeapon]))
            {
                ChangeEquipment(E_EquipmentType.Weapon, 1);
                return true;
            }
        }
        else if(!nowEquipments[(int)E_EquipmentType.Weapon].isInUse)
        {
            if (nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] != 0 &&
                ListEquipments[(int)E_EquipmentType.Weapon][0] != null &&
                Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.FirstWeapon]))
            {
                ChangeEquipment(E_EquipmentType.Weapon, 0);
                return true;
            }
            
            if (nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] != 1 &&
                ListEquipments[(int)E_EquipmentType.Weapon][1] != null &&
                Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.SecondWeapon]))
            {
                ChangeEquipment(E_EquipmentType.Weapon, 1);
                return true;
            }
        }

        // 卸武器
        if (nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] != -1 &&
            !nowEquipments[(int)E_EquipmentType.Weapon].isInUse &&
            Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.CancelWeapon]))
        {
            RemoveEquipment(E_EquipmentType.Weapon);
            return true;
        }

        return false;
    }

    // 动画事件-----
    public void HoldWeapon()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).HoldWeapon();
        }
    }

    public void ReleaseWeapon(float time)
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).ReleaseWeapon(time);
        }
    }

    public void OpenCheckWeaponIsHit(int isAllowRotate)
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).OpenCheckWeaponIsHit(isAllowRotate);
        }
    }
    public void CloseCheckWeaponIsHit()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).CloseCheckWeaponIsHit();
        }
    }

    public void ResetIsContinueAttack(float time)
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).ResetIsContinueAttack(time);
        }
    }

    public void StartDefense()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).StartDefense();
        }
    }

    public void EndDefense()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).EndDefense();
        }
    }

    public void StartTakeWeaponAnimation()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).StartTakeWeaponAnimation();
        }
    }

    public void EndTakeWeaponAnimation()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).EndTakeWeaponAnimation();
        }
    }

    public void StartUnTakeWeaponAnimation()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).StartUnTakeWeaponAnimation();
        }
    }

    public void EndUnTakeWeaponAnimation()
    {
        if (nowEquipments[(int)E_EquipmentType.Weapon] is Weapon)
        {
            (nowEquipments[(int)E_EquipmentType.Weapon] as Weapon).EndUnTakeWeaponAnimation();
        }
    }
}
