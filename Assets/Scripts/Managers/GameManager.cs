using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    public List<MonsterStateMachine> listMonsters;
    
    // 上一次查找最近距离的怪物的最近距离
    public float lastTimeNearestMonsterNearestDistance;
    // 上一次查找最近的怪物的Transform组件
    public Transform lastTimeNearestMonsterTransform;
    // 上一次查找最近的怪物的MonsterStateMachine
    public MonsterStateMachine lastTimeNearestMonster;

    private int _count;
    // 查找最近距离怪物时用的最近距离怪物索引值
    private int _nearestMonsterIndex;
    private void Awake()
    {
        if (_instance is null)
        {
            _instance = this;
        }

        listMonsters = new List<MonsterStateMachine>();
    }
    
    // 获取距离玩家最近的怪物的距离
    public float GetNearestMonster()
    {
        _nearestMonsterIndex = 0;
        lastTimeNearestMonsterNearestDistance = listMonsters[0].playerDistance;
        for (_count = 1; _count < listMonsters.Count; _count++)
        {
            if (listMonsters[_count].playerDistance < lastTimeNearestMonsterNearestDistance)
            {
                _nearestMonsterIndex = _count;
                lastTimeNearestMonsterNearestDistance = listMonsters[_count].playerDistance;
            }
        }

        lastTimeNearestMonster = listMonsters[_nearestMonsterIndex];
        lastTimeNearestMonsterTransform = lastTimeNearestMonster.transform;
        return lastTimeNearestMonsterNearestDistance;
    }
    
    // 游戏对象生成(去与对象池交互)
    
    // 控制游戏中道具、怪物总数量等
    
    // 游戏初始化
    
    // 游戏场景切换
    
    // 游戏进行状态管理
}
