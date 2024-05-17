using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GuidingPointsGroup : MonoBehaviour
{
    private static GuidingPointsGroup _instance;
    public static GuidingPointsGroup Instance => _instance;
    
    public Image guidingPointImage;

    public List<GuidingPoint> guidingPoints;
    public int nowGuidingIndex;
    public GuidingPoint nowGuidingPoint;
    public UnityEvent whenAllGuidingPointIsTriggered;
    private int _count;
    private AsyncOperation _asyncLoad;

    public Transform loadPanel;
    public Image loadImage;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        nowGuidingIndex = -1;
        if (guidingPoints.Count != 0)
        {
            nowGuidingIndex = 0;
            guidingPointImage.gameObject.SetActive(true);
            nowGuidingPoint = guidingPoints[nowGuidingIndex];
            nowGuidingPoint.guidingPointsGroup = this;
            nowGuidingPoint.transform.gameObject.SetActive(true);
            for (_count = 1; _count < guidingPoints.Count; _count++)
            {
                guidingPoints[_count].guidingPointsGroup = this;
                guidingPoints[_count].transform.gameObject.SetActive(false);
            }
        }
        loadPanel.gameObject.SetActive(false);
        whenAllGuidingPointIsTriggered.AddListener(ChangeSceneToNext);
    }

    private void Update()
    {
        if (nowGuidingIndex != -1 && UICameraController.Instance != null)
        {
            guidingPointImage.transform.position =
                UICameraController.Instance.GetRelativeMainCameraToUICameraPosition(nowGuidingPoint.transform.position);
            guidingPointImage.transform.LookAt(UICameraController.Instance.transform);
        }
    }

    public void ChangeToNextPoint()
    {
        nowGuidingIndex++;
        
        if (nowGuidingIndex >= guidingPoints.Count)
        {
            guidingPointImage.gameObject.SetActive(false);
            nowGuidingIndex = -1;
            whenAllGuidingPointIsTriggered?.Invoke();
            return;
        }

        guidingPoints[nowGuidingIndex].transform.gameObject.SetActive(true);
        nowGuidingPoint = guidingPoints[nowGuidingIndex];
    }

    private void ChangeSceneToNext()
    {
        loadPanel.gameObject.SetActive(true);
        for (_count = 1; _count < 4; _count++)
        {
            loadPanel.GetChild(_count).gameObject.SetActive(false);
        }
        loadPanel.GetChild((int)UIPanelManager.Instance.theme + 1).gameObject.SetActive(true);
        loadImage = loadPanel.GetChild((int)UIPanelManager.Instance.theme + 1).GetChild(1).GetComponent<Image>();
        loadImage.fillAmount = 0f;
        StartCoroutine(LoadSceneAsync());
    }
    
    private IEnumerator LoadSceneAsync()
    {
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 1 :
                _asyncLoad = SceneManager.LoadSceneAsync(0);
                break;
            case 2 :
                _asyncLoad = SceneManager.LoadSceneAsync(0);
                break;
            case 3 :
                _asyncLoad = SceneManager.LoadSceneAsync(4);
                break;
            case 4 :
                _asyncLoad = SceneManager.LoadSceneAsync(0);
                break;
        }
        
        _asyncLoad.allowSceneActivation = false;
        
        // 等待场景加载完成
        while (!_asyncLoad.isDone)
        {
            loadImage.fillAmount = _asyncLoad.progress;
            if (_asyncLoad.progress >= 0.8f)
            {
                _asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        loadImage.fillAmount = 1f;
    }

    public Vector3 GetRecentPoint()
    {
        if (nowGuidingIndex == -1 || nowGuidingIndex == 0)
        {
            return transform.position;
        }
        else
        {
            return guidingPoints[nowGuidingIndex - 1].transform.position;
        }
    }
}
