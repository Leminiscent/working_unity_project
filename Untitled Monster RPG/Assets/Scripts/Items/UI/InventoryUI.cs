using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] private GameObject _itemList;
    [SerializeField] private ItemSlotUI _itemSlotUI;
    [SerializeField] private TextMeshProUGUI _categoryText;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TextMeshProUGUI _itemDescription;
    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private Image _upArrow;
    [SerializeField] private Image _downArrow;

    private int _selectedCategory;
    private const int ITEMS_IN_VIEWPORT = 10;
    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private RectTransform _itemListRect;
    private DummySelectionUI _categorySelectionUI;
    private List<int> _availableCategoryIndices;

    public ItemBase SelectedItem => _inventory.GetItem(_selectedItem, _selectedCategory);
    public int SelectedCategory => _selectedCategory;

    private void Awake()
    {
        _inventory = Inventory.GetInventory();
        _itemListRect = _itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _moneyText.text = $"{Wallet.Instance.Money} GP";
        _selectedCategory = GetFirstNonEmptyCategory();
        UpdateCategoriesAndItemList();
        _inventory.OnUpdated += UpdateCategoriesAndItemList;
        Wallet.Instance.OnMoneyChanged += UpdateMoneyText;
    }

    private void UpdateCategoriesAndItemList()
    {
        List<int> updatedCategories = new();
        for (int i = 0; i < Inventory.ItemCategories.Count; i++)
        {
            if (_inventory.GetSlotsByCategory(i).Count > 0)
            {
                updatedCategories.Add(i);
            }
        }
        _availableCategoryIndices = updatedCategories;

        if (!_availableCategoryIndices.Contains(_selectedCategory))
        {
            _selectedCategory = _availableCategoryIndices.Count > 0 ? _availableCategoryIndices[0] : 0;
        }
        _categoryText.text = Inventory.ItemCategories[_selectedCategory];

        if (_categorySelectionUI == null)
        {
            _categorySelectionUI = gameObject.AddComponent<DummySelectionUI>();
            _categorySelectionUI.IgnoreVerticalInput = true;
            _categorySelectionUI.OnIndexChanged += (index) =>
            {
                _selectedCategory = _availableCategoryIndices[index];
                _categoryText.text = Inventory.ItemCategories[_selectedCategory];
                ResetSelction();
                UpdateCategoriesAndItemList();
            };
        }
        _categorySelectionUI.SetSelectionSettings(SelectionType.Grid, _availableCategoryIndices.Count);
        List<DummySelectable> categoryItems = new();
        for (int i = 0; i < _availableCategoryIndices.Count; i++)
        {
            categoryItems.Add(new DummySelectable());
        }
        _categorySelectionUI.SetItems(categoryItems);
        int selIndex = _availableCategoryIndices.IndexOf(_selectedCategory);
        _categorySelectionUI.SetSelectedIndex(selIndex);

        foreach (Transform child in _itemList.transform)
        {
            Destroy(child.gameObject);
        }
        _slotUIList = new List<ItemSlotUI>();
        foreach (ItemSlot itemSlot in _inventory.GetSlotsByCategory(_selectedCategory))
        {
            ItemSlotUI slotUIObj = Instantiate(_itemSlotUI, _itemList.transform);
            slotUIObj.SetData(itemSlot);
            _slotUIList.Add(slotUIObj);
        }
        SetItems(_slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());
        UpdateSelectionInUI();
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

    private void ResetSelction()
    {
        _selectedItem = 0;
        _upArrow.gameObject.SetActive(false);
        _downArrow.gameObject.SetActive(false);
        _itemIcon.sprite = null;
        _itemDescription.text = "";
    }

    private int GetFirstNonEmptyCategory()
    {
        for (int i = 0; i < Inventory.ItemCategories.Count; i++)
        {
            if (_inventory.GetSlotsByCategory(i).Count > 0)
            {
                return i;
            }
        }
        return 0;
    }

    private void UpdateMoneyText()
    {
        _moneyText.text = $"{Wallet.Instance.Money} GP";
    }

    public void HideMoneyText()
    {
        _moneyText.gameObject.SetActive(false);
    }

    public void ShowMoneyText()
    {
        _moneyText.gameObject.SetActive(true);
    }

    public override void HandleUpdate()
    {
        if (_categorySelectionUI != null)
        {
            _categorySelectionUI.HandleUpdate();
        }
        base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        List<ItemSlot> slots = _inventory.GetSlotsByCategory(_selectedCategory);
        if (slots.Count > 0)
        {
            ItemBase item = slots[_selectedItem].Item;
            _itemIcon.sprite = item.Icon;
            _itemDescription.text = item.Description;
        }
        HandleScrolling();
        base.UpdateSelectionInUI();
    }

    public void ResetInventoryScreen()
    {
        _selectedCategory = GetFirstNonEmptyCategory();
        ResetSelction();
        UpdateCategoriesAndItemList();
    }
}
