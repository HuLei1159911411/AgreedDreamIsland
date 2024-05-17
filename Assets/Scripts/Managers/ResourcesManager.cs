using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ResourcesManager : MonoBehaviour
{
    private static ResourcesManager _instance;
    public static ResourcesManager Instance => _instance;
    
    private Dictionary<string,Object> _dicLoadedResources;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _dicLoadedResources = new Dictionary<string, Object>();
    }

    public T LoadObject<T>(string path) where T : UnityEngine.Object
    {
        if (!_dicLoadedResources.ContainsKey(path))
        {
            _dicLoadedResources.Add(path, Resources.Load<T>(path));
        }

        return _dicLoadedResources[path] as T;
    }
}
