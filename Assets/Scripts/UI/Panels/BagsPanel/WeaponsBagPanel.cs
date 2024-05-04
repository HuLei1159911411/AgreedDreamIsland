using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public struct WeaponCell
{
    public Transform weaponCellTransform;
    public Image weaponCellBackgroundImage;
    public Text weaponCellText;
}

public class WeaponsBagPanel : Panel
{
    public Transform alwaysShowWeaponCells;
    public Transform bagWeaponCells;
    public List<WeaponCell> listAlwaysShowWeaponCells;
    public List<WeaponCell> listBagWeaponCells;

    public Transform selectFirstWeaponTipIcon;
    public Text selectFirstWeaponTipIconKeyText;
    public Transform selectSecondWeaponTipIcon;
    public Text selectSecondWeaponTipIconKeyText;

    private Color _absentWeaponColor;
    private Color _existentWeaponColor;
    private Color _selectedWeaponColor;
    private Vector3 _selectedWeaponLocalScale;

    private int _count;
    private int _index;
    private int _weaponCellsCount;
    private int _nowWeaponIndex;
    private void Awake()
    {
        InitParams();
        InitAllWeaponCells();
        
        alwaysShowWeaponCells.gameObject.SetActive(true);
        bagWeaponCells.gameObject.SetActive(false);
    }

    private void Start()
    {
        EquipmentsController.Instance.weaponsBagPanel = this;
        SetWeaponsBagByEquipmentsController();
        
        UIPanelManager.Instance.listPanels.Add(this);
        UIPanelManager.Instance.PanelsInputEvent += () =>
        {
            if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.OpenBag]))
            {
                if (!isShow)
                {
                    UIPanelManager.Instance.CloseAllPanels();
                    ShowPanel();
                }
                else
                {
                    ClosePanel();
                }
            }
        };

        selectFirstWeaponTipIconKeyText.text = Regex.Replace(Enum.GetName(typeof(KeyCode),
            InputManager.Instance.DicBehavior[E_InputBehavior.FirstWeapon]), Regex.Escape("Alpha"), "");
        selectSecondWeaponTipIconKeyText.text = Regex.Replace(Enum.GetName(typeof(KeyCode),
            InputManager.Instance.DicBehavior[E_InputBehavior.SecondWeapon]), Regex.Escape("Alpha"), "");
        
        selectFirstWeaponTipIcon.gameObject.SetActive(false);
        selectSecondWeaponTipIcon.gameObject.SetActive(false);
    }

    public override void ClosePanel()
    {
        base.ClosePanel();
        
        bagWeaponCells.gameObject.SetActive(false);
    }

    public override void ShowPanel()
    {
        base.ShowPanel();

        bagWeaponCells.gameObject.SetActive(true);
    }

    private void InitParams()
    {
        _absentWeaponColor = Color.black;
        _absentWeaponColor.a = 118f / 255f;
        _existentWeaponColor = Color.white;
        _existentWeaponColor.a = _absentWeaponColor.a;
        _selectedWeaponColor = Color.white;
        _existentWeaponColor.a = 188f / 255f;
        _selectedWeaponLocalScale = new Vector3(1.03f, 1.03f, 1.03f);

        _weaponCellsCount = listBagWeaponCells.Count + listAlwaysShowWeaponCells.Count;
    }

    private void InitAllWeaponCells()
    {
        _nowWeaponIndex = -1;
        
        for (_count = 0; _count < listAlwaysShowWeaponCells.Count; _count++)
        {
            listAlwaysShowWeaponCells[_count].weaponCellTransform.localScale = Vector3.one;
            InitWeaponCell(listAlwaysShowWeaponCells[_count]);
        }

        for (_count = 0; _count < listBagWeaponCells.Count; _count++)
        {
            InitWeaponCell(listBagWeaponCells[_count]);
        }
    }

    public void SetWeaponsBagByEquipmentsController()
    {
        InitAllWeaponCells();
        
        // 在listAlwaysShowWeaponCells中设置存在装备的格子
        for (_index = 0;
             _index < (EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon].Count <= 2
                 ? EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon].Count
                 : 2);
             _index++)
        {
            if (EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon][_index] != null)
            {
                if (_index == 0)
                {
                    selectFirstWeaponTipIcon.gameObject.SetActive(true);
                }

                if (_index == 1)
                {
                    selectSecondWeaponTipIcon.gameObject.SetActive(true);
                }
                listAlwaysShowWeaponCells[_index].weaponCellText.text =
                    EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon][_index].strName;
                listAlwaysShowWeaponCells[_index].weaponCellBackgroundImage.color = _existentWeaponColor;
            }
            else
            {
                InitWeaponCell(listAlwaysShowWeaponCells[_index]);
            }
        }
        
        // 在listAlwaysShowWeaponCells中设置当前选中的武器
        SetNowWeaponCell(EquipmentsController.Instance.nowEquipmentsIndexes[(int)E_EquipmentType.Weapon]);

        _count = 0;
        while (_index < EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon].Count)
        {
            listBagWeaponCells[_count].weaponCellText.text =
                EquipmentsController.Instance.listEquipments[(int)E_EquipmentType.Weapon][_index++].strName;
            listBagWeaponCells[_count++].weaponCellBackgroundImage.color = _existentWeaponColor;
        }

        for (; _count < listBagWeaponCells.Count; _count++)
        {
            InitWeaponCell(listBagWeaponCells[_count]);
        }
    }

    public void InitWeaponCell(WeaponCell cell)
    {
        cell.weaponCellBackgroundImage.color = _absentWeaponColor;
        cell.weaponCellText.text = "";
    }

    public void SetNowWeaponCell(int nowWeaponIndex)
    {
        if (_nowWeaponIndex == nowWeaponIndex)
        {
            if (nowWeaponIndex == -1)
            {
                return;
            }
            
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellText.text =
                EquipmentsController.Instance.nowEquipments[(int)E_EquipmentType.Weapon].strName;
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellBackgroundImage.color = _selectedWeaponColor;
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellTransform.localScale = _selectedWeaponLocalScale;
            return;
        }

        if (nowWeaponIndex != -1)
        {
            // 已经有选中的装备了 
            if (_nowWeaponIndex != -1)
            {
                listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellBackgroundImage.color = _existentWeaponColor;
                listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellTransform.localScale = Vector3.one;
            }
            listAlwaysShowWeaponCells[nowWeaponIndex].weaponCellBackgroundImage.color = _selectedWeaponColor;
            listAlwaysShowWeaponCells[nowWeaponIndex].weaponCellTransform.localScale = _selectedWeaponLocalScale;
            _nowWeaponIndex = nowWeaponIndex;
        }
        else
        {
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellBackgroundImage.color = _existentWeaponColor;
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellTransform.localScale = Vector3.one;
        }
    }
}
