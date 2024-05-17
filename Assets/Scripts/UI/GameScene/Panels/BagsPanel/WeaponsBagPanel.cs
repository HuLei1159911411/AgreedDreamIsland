using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class WeaponCell
{
    public Transform weaponCellTransform;
    public Image weaponCellBackgroundImage;
    public Image weaponIcon;
    public Equipment weapon;
}

public class WeaponsBagPanel : Panel
{
    private static WeaponsBagPanel _instance;
    public static WeaponsBagPanel Instance => _instance;
    
    public Transform alwaysShowWeaponCells;
    public Transform bagWeaponCells;
    public List<WeaponCell> listAlwaysShowWeaponCells;
    public List<WeaponCell> listBagWeaponCells;

    public Transform selectFirstWeaponTipIcon;
    public Text selectFirstWeaponTipIconKeyText;
    public Transform selectSecondWeaponTipIcon;
    public Text selectSecondWeaponTipIconKeyText;

    public Transform discardArea;
    
    private Color _absentWeaponColor;
    public Color whiteLevelBackgroundColor;
    public Color blueLevelBackgroundColor;
    public Color purpleLevelBackgroundColor;
    public Color goldenLevelBackgroundColor;
    // private Color _existentWeaponColor;
    // private Color _selectedWeaponColor;
    private Vector3 _selectedWeaponLocalScale;

    private int _count;
    private int _index;
    private int _weaponCellsCount;
    private int _nowWeaponIndex;
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        InitParams();
        InitAllWeaponCells();
        
        alwaysShowWeaponCells.gameObject.SetActive(true);
        bagWeaponCells.gameObject.SetActive(false);
        discardArea.gameObject.SetActive(false);
    }

    private void Start()
    {
        EquipmentsController.Instance.weaponsBagPanel = this;
        SetWeaponsBagByEquipmentsController();
        
        UIPanelManager.Instance.listPanels.Add(this);
        UIPanelManager.Instance.PanelsInputEvent += ListenPanelShowAndClose;

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

        discardArea.gameObject.SetActive(false);
        InfoManager.Instance.isStopListenPlayerBehaviorInput = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        bagWeaponCells.gameObject.SetActive(false);
    }

    public override void ShowPanel()
    {
        base.ShowPanel();

        InfoManager.Instance.isStopListenPlayerBehaviorInput = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        bagWeaponCells.gameObject.SetActive(true);
    }

    private void InitParams()
    {
        _absentWeaponColor = Color.black;
        _absentWeaponColor.a = 118f / 255f;
        // _existentWeaponColor = Color.white;
        // _existentWeaponColor.a = _absentWeaponColor.a;
        // _selectedWeaponColor = Color.white;
        // _existentWeaponColor.a = 188f / 255f;
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
        // 在listAlwaysShowWeaponCells中设置存在装备的格子
        for (_index = 0;
             _index < 2;
             _index++)
        {
            if (_index == 0)
            {
                if (EquipmentsController.Instance.ListEquipments[(int)E_EquipmentType.Weapon][_index] != null)
                {
                    selectFirstWeaponTipIcon.gameObject.SetActive(true);
                }
                else
                {
                    selectFirstWeaponTipIcon.gameObject.SetActive(false);
                }
            }

            if (_index == 1)
            {
                if (EquipmentsController.Instance.ListEquipments[(int)E_EquipmentType.Weapon][_index] != null)
                {
                    selectSecondWeaponTipIcon.gameObject.SetActive(true);
                }
                else
                {
                    selectSecondWeaponTipIcon.gameObject.SetActive(false);
                }
            }
            
            listAlwaysShowWeaponCells[_index].weapon =
                EquipmentsController.Instance.ListEquipments[(int)E_EquipmentType.Weapon][_index];
            SetCellByWeapon(listAlwaysShowWeaponCells[_index]);
        }
        
        // 在listAlwaysShowWeaponCells中设置当前选中的武器
        SetNowEquippedWeaponCell(EquipmentsController.Instance.nowEquipmentsIndexes[(int)E_EquipmentType.Weapon]);

        _count = 0;
        while (_index < EquipmentsController.Instance.ListEquipments[(int)E_EquipmentType.Weapon].Length)
        {
            listBagWeaponCells[_count].weapon =
                EquipmentsController.Instance.ListEquipments[(int)E_EquipmentType.Weapon][_index];
            SetCellByWeapon(listBagWeaponCells[_count]);
            _count++;
            _index++;
        }
    }

    public void InitWeaponCell(WeaponCell cell)
    {
        cell.weaponCellBackgroundImage.color = _absentWeaponColor;
        cell.weaponIcon.gameObject.SetActive(false);
        cell.weapon = null;
    }

    public void SetNowEquippedWeaponCell(int nowWeaponIndex)
    {
        if (_nowWeaponIndex == nowWeaponIndex)
        {
            return;
        }

        if (nowWeaponIndex != -1)
        {
            // 已经有选中的装备了 
            if (_nowWeaponIndex != -1)
            {
                listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellTransform.localScale = Vector3.one;
            }
            listAlwaysShowWeaponCells[nowWeaponIndex].weaponCellTransform.localScale = _selectedWeaponLocalScale;
            _nowWeaponIndex = nowWeaponIndex;
        }
        else
        {
            listAlwaysShowWeaponCells[_nowWeaponIndex].weaponCellTransform.localScale = Vector3.one;
            _nowWeaponIndex = -1;
        }
    }

    private void SetCellByWeapon(WeaponCell cell)
    {
        if (cell.weapon == null)
        {
            InitWeaponCell(cell);
            return;
        }
        
        // 武器图片
        cell.weaponIcon.gameObject.SetActive(true);
        if ( cell.weapon.equipmentName == E_EquipmentName.GrapplingHookGears)
        {
            cell.weaponIcon.sprite = ResourcesManager.Instance.LoadObject<Sprite>("WeaponIcon/GrapplingHookGears");
        }
        else
        {
            cell.weaponIcon.sprite = ResourcesManager.Instance.LoadObject<Sprite>("WeaponIcon/" +
                Enum.GetName(typeof(E_EquipmentName),  cell.weapon.equipmentName) + 
                "_" + 
                (cell.weapon as Weapon).weaponModelIndex);
        }
                
        // 武器背景颜色
        switch (cell.weapon.equipmentLevel)
        {
            case E_EquipmentLevel.White:
                cell.weaponCellBackgroundImage.color = whiteLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Blue:
                cell.weaponCellBackgroundImage.color = blueLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Purple:
                cell.weaponCellBackgroundImage.color = purpleLevelBackgroundColor;
                break;
            case E_EquipmentLevel.Golden:
                cell.weaponCellBackgroundImage.color = goldenLevelBackgroundColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ListenPanelShowAndClose()
    {
        if (Input.GetKeyDown(InputManager.Instance.DicBehavior[E_InputBehavior.OpenBagPanel]))
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
    }
}
