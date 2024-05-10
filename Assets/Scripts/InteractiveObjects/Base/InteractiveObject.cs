using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public enum E_InteractiveObjectType
{
    Item,
}

public abstract class InteractiveObject : MonoBehaviour
{
    public InteractController interactController;
    public E_InteractiveObjectType type;
    public string strName;
}
