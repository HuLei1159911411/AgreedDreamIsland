using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputKeyUI : MonoBehaviour
{
    public E_InputBehavior inputBehavior;
    public Button changeKeyButton;
    public Text nowKeyText;
    public Text inputTipText;
    public RectTransform maskImageRectTransform;
    
    private void Awake()
    {
        changeKeyButton.onClick.AddListener(StartListenKeyInput);
    }

    private void Start()
    {
        SetNowKeyText();
    }

    public void SetNowKeyText()
    {
        nowKeyText.text = "“" + InputManager.Instance.DicBehavior[inputBehavior] + "”";
    }

    public void StartListenKeyInput()
    {
        maskImageRectTransform.gameObject.SetActive(true);
        inputTipText.text = "请按下”任意键“以替换当前键，按下“Esc”键取消";
        StartCoroutine(ListenKeyInput());
    }

    private IEnumerator ListenKeyInput()
    {
        while (true)
        {
            yield return null;
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.OpenSettingPanel]))
                {
                    inputTipText.text = "";
                    maskImageRectTransform.gameObject.SetActive(false);
                    yield break;
                }

                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        inputTipText.text = "";
                        InputManager.Instance.DicBehavior[inputBehavior] = keyCode;
                        SetNowKeyText();
                        maskImageRectTransform.gameObject.SetActive(false);
                        yield break;
                    }
                }
            }
        }
    }
}
