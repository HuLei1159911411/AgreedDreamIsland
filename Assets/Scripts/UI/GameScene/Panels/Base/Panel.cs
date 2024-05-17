using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Panel : MonoBehaviour
{
    public bool isShow;

    public virtual void ShowPanel()
    {
        isShow = true;
    }

    public virtual void ClosePanel()
    {
        isShow = false;
    }
}
