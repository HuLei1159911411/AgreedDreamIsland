using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefeatableCharacter : MonoBehaviour
{
    // 血量
    public float hp;
    // 受到攻击
    public virtual bool Hit(float damage, Vector3 hitPosition, ICounterattack counterattack, bool isStrongAttack)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Death();
        }

        return true;
    }
    // 死亡
    public abstract void Death();
}
