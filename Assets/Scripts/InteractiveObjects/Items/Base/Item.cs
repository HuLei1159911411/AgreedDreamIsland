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

    // 丢物品进行射线检测的射线的击中信息
    private RaycastHit discardItemDownRaycastHit;
    
    public virtual bool PickUpItem()
    {
        return false;
    }

    public virtual bool DiscardItem(Vector3 discardPosition)
    {
        if (Physics.Raycast(discardPosition, Vector3.down, out discardItemDownRaycastHit, 100f,
                InfoManager.Instance.layerGroundCheck))
        {
            transform.position = discardItemDownRaycastHit.point;
            transform.rotation *= Quaternion.FromToRotation(transform.up, discardItemDownRaycastHit.normal);
            itemCollider.enabled = true;
            return true;
        }
        else
        {
            return false;
        }
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
