using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_ObjectType
{
    Monster,
    MonsterWeapon,
    PlayerWeapon,
    MonsterHp,
}

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager _instance;
    public static ObjectPoolManager Instance => _instance;

    public GameObject monsterPrefab;
    public GameObject monsterWeaponPrefab;
    public GameObject playerWeaponPrefab;
    public GameObject monsterHpPrefab;

    private Dictionary<E_ObjectType, Queue<GameObject>> _dictionaryObjectPool;

    private GameObject _tempGameObject;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _dictionaryObjectPool = new Dictionary<E_ObjectType, Queue<GameObject>>();
    }

    public GameObject GetObject(E_ObjectType objectType)
    {
        if (_dictionaryObjectPool.ContainsKey(objectType) && _dictionaryObjectPool[objectType].Count > 0)
        {
            return _dictionaryObjectPool[objectType].Dequeue();
        }

        switch (objectType)
        {
            case E_ObjectType.Monster:
                return GameObject.Instantiate(monsterPrefab);
            case E_ObjectType.MonsterWeapon:
                return GameObject.Instantiate(monsterWeaponPrefab);
            case E_ObjectType.PlayerWeapon:
                return GameObject.Instantiate(playerWeaponPrefab);
            case E_ObjectType.MonsterHp:
                _tempGameObject = GameObject.Instantiate(monsterHpPrefab);
                _tempGameObject.transform.SetParent(UIPanelManager.Instance.lowLayer);
                _tempGameObject.transform.localScale = Vector3.one;
                return _tempGameObject;
            default:
                throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }

    public void RecyclingObject(E_ObjectType objectType, GameObject objGameObject)
    {
        objGameObject.SetActive(false);
        if (!_dictionaryObjectPool.ContainsKey(objectType))
        {
            _dictionaryObjectPool.Add(objectType, new Queue<GameObject>());
        }
        _dictionaryObjectPool[objectType].Enqueue(objGameObject);
    }
}
