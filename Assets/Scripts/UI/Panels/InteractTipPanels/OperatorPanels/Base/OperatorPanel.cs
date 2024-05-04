using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public struct OperatorObject
{
    public RectTransform operatorTransform;
    public RectTransform operatorIconTransform;
    public Image operatorIconImage;
    public Text operatorIconText;
    public Text operatorText;
    public bool isLongPress;
    public float longPressTime;
    public delegate Func<bool> ExecuteOperator();
}

public abstract class OperatorPanel : Panel
{
    public Image backgroundImage;
    public Text interactiveObjectName;
    
    public List<OperatorObject> listOperatorObjects;
    public abstract void UpdateOperatorInformation(InteractiveObject interactiveObject);
    public abstract bool ListenInteractOperators();
}
