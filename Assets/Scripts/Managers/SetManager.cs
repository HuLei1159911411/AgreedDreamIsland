using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetManager : MonoBehaviour
{
    private static SetManager _instance;
    public static SetManager Instance => _instance;

    // 开启音效
    private bool _isOpenSoundEffect;
    // 音效声大小
    private float _soundEffectVolume;
    // 开启背景音乐
    private bool _isOpenBackgroundMusic;
    // 背景声大小
    private float _backgroundMusicVolume;
    // 鼠标速度
    private float _mouseSpeed;
    // 设置界面
    public SettingPanel settingPanel;
    public bool IsOpenSoundEffect => _isOpenSoundEffect;
    public bool IsOpenBackgroundMusic => _isOpenBackgroundMusic;
    public float SoundEffectVolume => _soundEffectVolume;
    public float BackgroundMusicVolume => _backgroundMusicVolume;
    public float MouseSpeed => _mouseSpeed;

    public UnityEvent whenSoundEffectChange;
    public UnityEvent whenBackgroundMusicChange;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _isOpenSoundEffect = true;
        _isOpenBackgroundMusic = true;
        
        // 加入数据本地化后将这里的值从本地加载
        _isOpenSoundEffect = true;
        _soundEffectVolume = 0.5f;
        _isOpenBackgroundMusic = true;
        _backgroundMusicVolume = 0.5f;
        _mouseSpeed = 1f;
    }

    // 再重新加载场景后的初始化
    public void Init()
    {
        whenSoundEffectChange.RemoveAllListeners();
    }

    public void SetSoundEffectVolume(float value)
    {
        _soundEffectVolume = value;

        whenSoundEffectChange?.Invoke();
    }

    public void SetBackgroundMusicVolume(float value)
    {
        _backgroundMusicVolume = value;
        
        whenBackgroundMusicChange?.Invoke();
    }

    public void ChangeSoundEffectOpen(bool value)
    {
        _isOpenSoundEffect = value;
        
        whenSoundEffectChange?.Invoke();
    }

    public void ChangeBackgroundMusicOpen(bool value)
    {
        _isOpenBackgroundMusic = value;
        
        whenBackgroundMusicChange?.Invoke();
    }

    public void ChangeMouseSpeed(float value)
    {
        _mouseSpeed = value;
        if (CameraController.Instance != null)
        {
            CameraController.Instance.mouseSpeed = value * 50f;
        }
    }
}
