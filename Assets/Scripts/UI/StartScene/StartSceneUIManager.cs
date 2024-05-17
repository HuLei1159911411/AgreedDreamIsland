using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum E_Theme
{
    Snow,
    Lava,
    Wood,
}

public class StartScene : MonoBehaviour
{
    public E_Theme theme;
    
    public Transform snowTransform;
    public Transform lavaTransform;
    public Transform woodTransform;
    public Transform loadPanelTransform;
    public Transform snowLoadTransform;
    public Transform lavaLoadTransform;
    public Transform woodLoadTransform;
    
    private Transform _nowTransform;
    private Image _backgroundImage;
    private Button _startGameButton;
    private Button _settingButton;
    private Button _quitButton;
    private Transform _nowLoadTransform;
    private Image _loadImage;
    private AsyncOperation _asyncLoad;
    public void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        snowTransform.gameObject.SetActive(false);
        lavaTransform.gameObject.SetActive(false);
        woodTransform.gameObject.SetActive(false);
        snowLoadTransform.gameObject.SetActive(false);
        lavaLoadTransform.gameObject.SetActive(false);
        woodLoadTransform.gameObject.SetActive(false);
        switch (theme)
        {
            case E_Theme.Snow:
                _nowTransform = snowTransform;
                _nowLoadTransform = snowLoadTransform;
                break;
            case E_Theme.Lava:
                _nowTransform = lavaTransform;
                _nowLoadTransform = lavaLoadTransform;
                break;
            case E_Theme.Wood:
                _nowTransform = woodTransform;
                _nowLoadTransform = woodLoadTransform;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _nowTransform.gameObject.SetActive(true);
        _nowLoadTransform.gameObject.SetActive(true);
        _backgroundImage = _nowTransform.GetChild(0).GetComponent<Image>();
        _startGameButton = _nowTransform.GetChild(1).GetComponent<Button>();
        _settingButton = _nowTransform.GetChild(2).GetComponent<Button>();
        _quitButton = _nowTransform.GetChild(3).GetComponent<Button>();
        _loadImage = _nowLoadTransform.GetChild(1).GetComponent<Image>();
        _loadImage.fillAmount = 0f;
        
        loadPanelTransform.gameObject.SetActive(false);

        _startGameButton.onClick.AddListener(ChangeScene);
        
        _quitButton.onClick.AddListener(QuitGame);
    }

    private void ChangeScene()
    {
        loadPanelTransform.gameObject.SetActive(true);
        StartCoroutine(LoadSceneAsync());
    }
    
    private IEnumerator LoadSceneAsync()
    {
        _asyncLoad = SceneManager.LoadSceneAsync((int)theme + 1);
        _asyncLoad.allowSceneActivation = false;
        
        // 等待场景加载完成
        while (!_asyncLoad.isDone)
        {
            _loadImage.fillAmount = _asyncLoad.progress;
            if (_asyncLoad.progress >= 0.8f)
            {
                _asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        _loadImage.fillAmount = 1f;
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
