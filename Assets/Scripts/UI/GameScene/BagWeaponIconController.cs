using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BagWeaponIconController : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public int index;
    private Vector3 _startPosition;
    private Vector2 _localPosition;
    private GraphicRaycaster _graphicRaycaster;
    private List<RaycastResult> _results;
    private Image _image;
    private BagWeaponIconController _otherWeaponIcon;
    private int _count;
    private void Awake()
    {
        _results = new List<RaycastResult>();
        _image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = transform.position;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,
            eventData.position, UICameraController.Instance.uiCamera, out _localPosition);
        
        transform.localPosition = _localPosition;
        
        WeaponsBagPanel.Instance.discardArea.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform,
            eventData.position, UICameraController.Instance.uiCamera, out _localPosition);
        transform.localPosition = _localPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // if (EquipmentsController.Instance.nowEquipmentsIndexes[(int)E_EquipmentType.Weapon] == index)
        // {
        //     transform.position = _startPosition;
        //     _image.raycastTarget = true;
        //     return;
        // }
        
        _image.raycastTarget = false;
        _results.Clear();
        UIPanelManager.Instance.graphicRaycaster.Raycast(eventData, _results);
        
        if (_results.Count != 0)
        {
            if (_results[0].gameObject.name == "WeaponIcon")
            {
                _otherWeaponIcon = _results[0].gameObject.GetComponent<BagWeaponIconController>();
                EquipmentsController.Instance.ExchangeEquipment(E_EquipmentType.Weapon, index, _otherWeaponIcon.index);
            }
            else if(_results[0].gameObject.name == "BackgroundImage")
            {
                _otherWeaponIcon = _results[0].gameObject.transform.GetChild(1).GetComponent<BagWeaponIconController>();
                EquipmentsController.Instance.ExchangeEquipment(E_EquipmentType.Weapon, index, _otherWeaponIcon.index);
            }
            else if (_results[0].gameObject.name == "DiscardAreaImage")
            {
                EquipmentsController.Instance.DiscardEquipment(E_EquipmentType.Weapon, index);
            }
        }
        transform.position = _startPosition;
        _image.raycastTarget = true;
        WeaponsBagPanel.Instance.discardArea.gameObject.SetActive(false);
    }
    
}
