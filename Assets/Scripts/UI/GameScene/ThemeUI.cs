using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThemeUI : MonoBehaviour
{
    public string path;
    public Image changeableImage;
    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            path = Enum.GetName(typeof(E_Theme), StartScene.Instance.theme) + "/" + path;
        }
        else
        {
            path = Enum.GetName(typeof(E_Theme), UIPanelManager.Instance.theme) + "/" + path;
        }
        
        changeableImage.sprite = (ResourcesManager.Instance.LoadObject<Sprite>(path));
    }
}
