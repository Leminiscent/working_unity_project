using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util.GenericSelectionUI;

public class ShopUI : SelectionUI<TextSlot>
{
    [SerializeField] private GameObject _itemList;
    [SerializeField] private ItemSlotUI _itemSlotUI;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private Image _upArrow;
    [SerializeField] private Image _downArrow;

    private const int ITEMS_IN_VIEWPORT = 8;

    private List<ItemBase> _availableItems;
    private Action<ItemBase> _onItemSelected;
    private Action _onBack;
    private List<ItemSlotUI> _slotUIList;
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
        ResetSelection();
        UpdateItemList();

        SetSelectionSettings(SelectionType.List, 1);

        OnSelected += HandleItemSelected;
        OnBack += HandleBack;

        EnableInput(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnSelected -= HandleItemSelected;
        OnBack -= HandleBack;
    }

    private void UpdateItemList()
    {
        ClearItemList();

        _slotUIList = new List<ItemSlotUI>();
        foreach (ItemBase item in _availableItems)
        {
            ItemSlotUI slotUIObj = Instantiate(_itemSlotUI, _itemList.transform);
            slotUIObj.SetData(item);
            _slotUIList.Add(slotUIObj);
        }

        // Get the TextSlot components from each ItemSlotUI.
        List<TextSlot> textSlots = _slotUIList.Select(static s => s.GetComponent<TextSlot>()).ToList();
        SetItems(textSlots);
        UpdateSelectionInUI();
    }

    private void ClearItemList()
    {
        foreach (Transform child in _itemList.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void HandleItemSelected(int selection)
    {
        if (selection < 0 || selection >= _availableItems.Count)
        {
            return;
        }

        _onItemSelected?.Invoke(_availableItems[selection]);
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private void HandleBack()
    {
        _onBack?.Invoke();
    }

    public override void UpdateSelectionInUI()
    {
        int sel = Mathf.Clamp(_selectedItem, 0, _availableItems.Count - 1);
        UpdateSelectedItemDisplay(sel);
        HandleScrolling();
        base.UpdateSelectionInUI();
    }

    private void UpdateSelectedItemDisplay(int index)
    {
        if (_availableItems.Count > 0)
        {
            ItemBase item = _availableItems[index];
            _itemIcon.sprite = item.Icon;
            _itemDescription.text = item.Description;
        }
    }

    private void HandleScrolling()
    {
        if (_slotUIList == null || _slotUIList.Count <= ITEMS_IN_VIEWPORT)
        {
            _upArrow.gameObject.SetActive(false);
            _downArrow.gameObject.SetActive(false);
            return;
        }

        int maxScrollIndex = _slotUIList.Count - ITEMS_IN_VIEWPORT;
        float scrollOffset = Mathf.Clamp(_selectedItem - (ITEMS_IN_VIEWPORT / 2), 0, maxScrollIndex);
        float scrollPos = scrollOffset * _slotUIList[0].Height;
        _itemListRect.localPosition = new Vector2(_itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = _selectedItem > ITEMS_IN_VIEWPORT / 2;
        bool showDownArrow = _selectedItem < maxScrollIndex + (ITEMS_IN_VIEWPORT / 2);

        if (_upArrow.gameObject.activeSelf != showUpArrow)
        {
            _ = StartCoroutine(ObjectUtil.ScaleInOut(_upArrow.gameObject, showUpArrow));
        }
        if (_downArrow.gameObject.activeSelf != showDownArrow)
        {
            _ = StartCoroutine(ObjectUtil.ScaleInOut(_downArrow.gameObject, showDownArrow));
        }
    }
}