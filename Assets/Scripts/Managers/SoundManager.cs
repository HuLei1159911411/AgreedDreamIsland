using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum E_SoundEffectType
{
    Wave,
    Hit,
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    public AudioSource backgroundMusic;
    
    private GameObject _nowSoundObject;
    private AudioSource _nowAudioSource;
    private SoundEffect _soundEffect;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Start()
    {
        // 加载背景音乐文件
        if(SceneManager.GetActiveScene().buildIndex != 0)
        {
            backgroundMusic.clip =
                ResourcesManager.Instance.LoadObject<AudioClip>("AudioClip/BackgroundMusic" +
                                                                UIPanelManager.Instance.theme);
        }
        else
        {
            backgroundMusic.clip = ResourcesManager.Instance.LoadObject<AudioClip>("AudioClip/BackgroundMusic" + StartScene.Instance.theme);
        }

        SetBackgroundMusic(backgroundMusic);
        
        backgroundMusic.Play();
    }

    public void InstantiateSoundEffect(Vector3 position, E_SoundEffectType soundEffectType)
    {
        _nowSoundObject = ObjectPoolManager.Instance.GetObject(E_ObjectType.SoundEffect);
        _nowAudioSource = _nowSoundObject.GetComponent<AudioSource>();
        _soundEffect = _nowSoundObject.GetComponent<SoundEffect>();
        _nowAudioSource.clip = ResourcesManager.Instance.LoadObject<AudioClip>("AudioClip/" + soundEffectType);
        _soundEffect.playTime = _nowAudioSource.clip.length;
        _nowSoundObject.SetActive(true);
        _nowAudioSource.Play();
    }

    public void SetSoundEffect(AudioSource audioSource)
    {
        audioSource.volume = SetManager.Instance.SoundEffectVolume;
        audioSource.mute = !SetManager.Instance.IsOpenSoundEffect;
        SetManager.Instance.whenSoundEffectChange.AddListener(() =>
        {
            audioSource.volume = SetManager.Instance.SoundEffectVolume;
            audioSource.mute = !SetManager.Instance.IsOpenSoundEffect;
        });
    }

    public void SetBackgroundMusic(AudioSource audioSource)
    {
        backgroundMusic.volume = SetManager.Instance.BackgroundMusicVolume;
        backgroundMusic.mute = !SetManager.Instance.IsOpenBackgroundMusic;
        SetManager.Instance.whenBackgroundMusicChange.AddListener(() =>
        {
            backgroundMusic.volume = SetManager.Instance.BackgroundMusicVolume;
            backgroundMusic.mute = !SetManager.Instance.IsOpenBackgroundMusic;
        });
    }
}
