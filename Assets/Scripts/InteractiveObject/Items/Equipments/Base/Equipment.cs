using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum E_EquipmentName
{
    GrapplingHookGears,
    Rod,
}

[Serializable]
public abstract class Equipment : Item
{
    public EquipmentsController controller;
    public E_EquipmentName equipmentName;
    public E_EquipmentType equipmentType;
    public bool isInUse;
    
    public Equipment()
    {
        itemType = E_ItemType.Equipment;
    }
    
    public abstract bool WearEquipment();
    public abstract bool RemoveEquipment();
    public abstract void ListenEquipmentUse();
}
