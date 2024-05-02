using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum E_EquipmentName
{
    GrapplingHookGears,
}

public abstract class Equipment : Item
{
    public EquipmentsController controller;
    public E_EquipmentName equipmentName;
    public E_EquipmentType equipmentType;
    
    
    public abstract bool WearEquipment();
    public abstract bool RemoveEquipment();
    public abstract void ListenEquipmentUse();
}
