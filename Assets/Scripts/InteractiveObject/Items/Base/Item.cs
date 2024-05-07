using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_ItemType
{
    Equipment,
    Supply,
}

public class Item : InteractiveObject
{
    public E_ItemType itemType;
    
    [HideInInspector] public Collider itemCollider;

    public virtual bool PickUpItem()
    {
        return false;
    }

    public virtual bool DiscardItem()
    {
        if (!(PlayerMovementStateMachine.Instance is null))
        {
            transform.rotation = Quaternion.identity;
            transform.position = PlayerMovementStateMachine.Instance.GetGroundPoint();
            itemCollider.enabled = true;
            return true;
        }
        
        return false;
    }

    public virtual bool UseItem()
    {
        return false;
    }

    public Item()
    {
        type = E_InteractiveObjectType.Item;
    }
}
