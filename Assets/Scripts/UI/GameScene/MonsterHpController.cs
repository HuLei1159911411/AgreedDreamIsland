using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterHpController : MonoBehaviour
{
    public MonsterCharacter monsterCharacter;
    public Vector3 hpPositionOffset;
    private Vector3 _nowHpPositionOffset;
    
    public RectTransform hpBackgroundRectTransform;
    public RectTransform nowHpRectTransform;
    private Vector2 _calHp;

    public void Init()
    {
        _nowHpPositionOffset = monsterCharacter.transform.localScale.x * hpPositionOffset;
    }
    
    public void UpdatePosition()
    {
        transform.position =
            UICameraController.Instance.GetRelativeMainCameraToUICameraPosition(monsterCharacter.transform.position) +
            _nowHpPositionOffset;
    }
    
    public void UpdateHpUI()
    {
        _calHp = nowHpRectTransform.offsetMax;
        _calHp.x = -(1f - monsterCharacter.hp / monsterCharacter.maxHp) * hpBackgroundRectTransform.rect.width;

        nowHpRectTransform.offsetMax = _calHp;
    }
}
