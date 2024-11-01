using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject _itemList;
    [SerializeField] private ItemSlotUI _itemSlotUI;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private Image _upArrow;
    [SerializeField] private Image _downArrow;

    private int _selectedItem;
    private List<ItemBase> _availableItems;
    private Action<ItemBase> _onItemSelected;
    private Action _onBack;
    private List<ItemSlotUI> _slotUIList;
    private const int ITEMS_IN_VIEWPORT = 8;
    private RectTransform _itemListRect;

    private void Awake()
    {
        _itemListRect = _itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        _availableItems = availableItems;
        _onItemSelected = onItemSelected;
        _onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        int prevSelection = _selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++_selectedItem;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --_selectedItem;
        }
        _selectedItem = Mathf.Clamp(_selectedItem, 0, _availableItems.Count - 1);
        if (prevSelection != _selectedItem)
        {
            UpdateItemSelection();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _onItemSelected?.Invoke(_availableItems[_selectedItem]);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            _selectedItem = 0;
            _onBack?.Invoke();
        }
    }

    private void UpdateItemList()
    {
        foreach (Transform child in _itemList.transform)
        {
            Destroy(child.gameObject);
        }

        _slotUIList = new List<ItemSlotUI>();
        foreach (ItemBase item in _availableItems)
        {
            ItemSlotUI slotUIObj = Instantiate(_itemSlotUI, _itemList.transform);

            slotUIObj.SetNameAndPrice(item);
            _slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    private void UpdateItemSelection()
    {
        _selectedItem = Mathf.Clamp(_selectedItem, 0, _availableItems.Count - 1);
        for (int i = 0; i < _slotUIList.Count; i++)
        {
            _slotUIList[i].NameText.color = i == _selectedItem ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }

        if (_availableItems.Count > 0)
        {
            ItemBase item = _availableItems[_selectedItem];

            _itemIcon.sprite = item.Icon;
            _itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if (_slotUIList.Count <= ITEMS_IN_VIEWPORT)
        {
            return;
        }

        int maxScrollIndex = _slotUIList.Count - ITEMS_IN_VIEWPORT;
        float scrollPos = Mathf.Clamp(_selectedItem - (ITEMS_IN_VIEWPORT / 2), 0, maxScrollIndex) * _slotUIList[0].Height;
        bool showUpArrow = _selectedItem > ITEMS_IN_VIEWPORT / 2;
        bool showDownArrow = _selectedItem < maxScrollIndex + (ITEMS_IN_VIEWPORT / 2);

        _itemListRect.localPosition = new Vector2(_itemListRect.localPosition.x, scrollPos);
        _upArrow.gameObject.SetActive(showUpArrow);
        _downArrow.gameObject.SetActive(showDownArrow);
    }
}
