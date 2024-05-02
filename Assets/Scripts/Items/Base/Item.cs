using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public abstract bool PickUpItem();
    public abstract bool DiscardItem();
    public abstract bool UseItem();
}
