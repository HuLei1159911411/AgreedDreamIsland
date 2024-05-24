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
    SoundEffect,
}

public class ObjectPoolManager : MonoBehaviour
{
    private static ObjectPoolManager _instance;
    public static ObjectPoolManager Instance => _instance;

    public GameObject monsterPrefab;
    public GameObject monsterWeaponPrefab;
    public GameObject playerWeaponPrefab;
    public GameObject monsterHpPrefab;
    public GameObject soundEffectPrefab;

    private Dictionary<E_ObjectType, Queue<GameObject>> _dictionaryObjectPool;

    private GameObject _tempMonsterHpGameObject;
    private GameObject _tempSoundEffectGameObject;
    private SoundEffect _tempSoundEffect;
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
                _tempMonsterHpGameObject = GameObject.Instantiate(monsterHpPrefab);
                _tempMonsterHpGameObject.transform.SetParent(UIPanelManager.Instance.lowLayer);
                _tempMonsterHpGameObject.transform.localScale = Vector3.one;
                return _tempMonsterHpGameObject;
            case E_ObjectType.SoundEffect:
                _tempSoundEffectGameObject = GameObject.Instantiate(soundEffectPrefab);
                _tempSoundEffect = _tempSoundEffectGameObject.GetComponent<SoundEffect>();
                SoundManager.Instance.SetSoundEffect(_tempSoundEffect.audioSource);
                return _tempSoundEffectGameObject;
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
