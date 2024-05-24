using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingPanel : Panel
{
    public Button closePanelButton;
    
    public Toggle soundSettingToggle;
    public Toggle inputKeySettingToggle;
    
    public RectTransform soundSettingRectTransform;
    public Toggle soundEffectToggle;
    public Toggle backgroundMusicToggle;
    public Slider soundEffectVolumeSlider;
    public Slider backgroundMusicVolumeSlider;
    public RectTransform inputKeySettingRectTransform;
    public Text inputKeyTipsText;

    public Slider mouseSpeedSlider;
    public Text mouseSpeedText;
    private void Awake()
    {
        if (SetManager.Instance != null)
        {
            SetManager.Instance.settingPanel = this;
        }
        
        closePanelButton.onClick.AddListener(ClosePanel);
        
        soundSettingToggle.onValueChanged.AddListener((value) =>
        {
            soundSettingRectTransform.gameObject.SetActive(value);
        });
        inputKeySettingToggle.onValueChanged.AddListener((value) =>
        {
            inputKeySettingRectTransform.gameObject.SetActive(value);
        });

        inputKeyTipsText.text = "";
        inputKeySettingRectTransform.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (UIPanelManager.Instance != null)
        {
            UIPanelManager.Instance.listPanels.Add(this);
            UIPanelManager.Instance.PanelsInputEvent += ChangeSettingPanelShow;
        }

        if (SetManager.Instance != null)
        {
            SetSettingPanel();
            soundEffectToggle.onValueChanged.AddListener(SetManager.Instance.ChangeSoundEffectOpen);
            soundEffectVolumeSlider.onValueChanged.AddListener(SetManager.Instance.SetSoundEffectVolume);
            backgroundMusicToggle.onValueChanged.AddListener(SetManager.Instance.ChangeBackgroundMusicOpen);
            backgroundMusicVolumeSlider.onValueChanged.AddListener(SetManager.Instance.SetBackgroundMusicVolume);
            mouseSpeedSlider.onValueChanged.AddListener((value) =>
            {
                mouseSpeedText.text = value.ToString("F1");
                SetManager.Instance.ChangeMouseSpeed(value);
            });
        }

        ClosePanel();
    }

    private void ChangeSettingPanelShow()
    {
        if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.OpenSettingPanel]))
        {
            if (isShow)
            {
                ClosePanel();
            }
            else
            {
                ShowPanel();
            }
        }
    }
    
    public override void ShowPanel()
    {
        base.ShowPanel();
        Time.timeScale = 0;
        
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        this.gameObject.SetActive(true);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        Time.timeScale = 1f;
        
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        SetManager.Instance.settingPanel = null;
    }

    public void SetSettingPanel()
    {
        soundEffectToggle.isOn = SetManager.Instance.IsOpenSoundEffect;
        backgroundMusicToggle.isOn = SetManager.Instance.IsOpenBackgroundMusic;
        soundEffectVolumeSlider.value = SetManager.Instance.SoundEffectVolume;
        backgroundMusicVolumeSlider.value = SetManager.Instance.BackgroundMusicVolume;
        mouseSpeedText.text = SetManager.Instance.MouseSpeed.ToString("F1");
        mouseSpeedSlider.value = SetManager.Instance.MouseSpeed;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackStartScene()
    {
        GuidingPointsGroup.Instance.ChangeSceneToNext();
    }
}
