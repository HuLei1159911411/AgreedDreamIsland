using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerCharacterInformationPanel : Panel
{
    public PlayerCharacter playerCharacter;
    
    public Transform hpTransform;
    public RectTransform hpBackgroundRectTransform;
    public RectTransform nowHpRectTransform;
    public Transform staminaTransform;
    public RectTransform staminaBackgroundRectTransform;
    public RectTransform nowStaminaRectTransform;

    private Vector2 _calHp;
    private Vector2 _calStamina;
    
    public void Start()
    {
        UIPanelManager.Instance.listPanels.Add(this);
        playerCharacter = PlayerCharacter.Instance;
        playerCharacter.whenStaminaChange += UpdateStaminaUI;
        playerCharacter.whenHpChange += UpdateHpUI;
        UpdateStaminaUI();
        UpdateHpUI();
        
        hpTransform.gameObject.SetActive(true);
        hpBackgroundRectTransform.gameObject.SetActive(true);
        nowHpRectTransform.gameObject.SetActive(true);
        staminaTransform.gameObject.SetActive(true);
        staminaBackgroundRectTransform.gameObject.SetActive(true);
        nowStaminaRectTransform.gameObject.SetActive(true);
    }
    
    public override void ShowPanel()
    {
        base.ShowPanel();
        this.gameObject.SetActive(true);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        this.gameObject.SetActive(false);
    }

    public void UpdateHpUI()
    {
        _calHp = nowHpRectTransform.offsetMax;
        _calHp.x = -(1f - playerCharacter.hp / playerCharacter.maxHp) * hpBackgroundRectTransform.rect.width;

        nowHpRectTransform.offsetMax = _calHp;
    }

    public void UpdateStaminaUI()
    {
        _calStamina = nowStaminaRectTransform.offsetMax;
        _calStamina.x = -(1f - playerCharacter.stamina / playerCharacter.maxStamina) * staminaBackgroundRectTransform.rect.width;

        nowStaminaRectTransform.offsetMax = _calStamina;
    }
}
