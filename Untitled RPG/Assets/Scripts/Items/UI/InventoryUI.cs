using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util.GenericSelectionUI;

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

    private const int ITEMS_IN_VIEWPORT = 10;
    private List<ItemSlotUI> _slotUIList;
    private Inventory _inventory;
    private RectTransform _itemListRect;
    private DummySelectionUI _categorySelectionUI;
    private List<int> _availableCategoryIndices;

    public ItemBase SelectedItem => _inventory.GetItem(_selectedItem, SelectedCategory);
    public int SelectedCategory { get; private set; }

    private void Awake()
    {
        _inventory = Inventory.GetInventory();
        _itemListRect = _itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        _moneyText.text = $"{Wallet.Instance.Money} GP";
        SelectedCategory = GetFirstNonEmptyCategory();
        UpdateCategoriesAndItemList();
        _inventory.OnUpdated += ResetInventoryScreen;
        Wallet.Instance.OnMoneyChanged += UpdateMoneyText;
    }

    private void OnDestroy()
    {
        _inventory.OnUpdated -= ResetInventoryScreen;
        Wallet.Instance.OnMoneyChanged -= UpdateMoneyText;
        if (_categorySelectionUI != null)
        {
            _categorySelectionUI.OnIndexChanged -= OnCategoryChanged;
        }
    }

    private void UpdateCategoriesAndItemList()
    {
        UpdateAvailableCategories();
        UpdateSelectedCategory();
        UpdateCategoryText();
        SetupCategorySelectionUI();
        BuildItemList();
        SetItems(_slotUIList.Select(static s => s.GetComponent<TextSlot>()).ToList());
        UpdateSelectionInUI();
    }

    private void UpdateAvailableCategories()
    {
        _availableCategoryIndices = new List<int>();
        for (int i = 0; i < Inventory.ItemCategories.Count; i++)
        {
            if (_inventory.GetSlotsByCategory(i).Count > 0)
            {
                _availableCategoryIndices.Add(i);
            }
        }
    }

    private void UpdateSelectedCategory()
    {
        if (!_availableCategoryIndices.Contains(SelectedCategory))
        {
            SelectedCategory = _availableCategoryIndices.Count > 0 ? _availableCategoryIndices[0] : 0;
        }
    }

    private void UpdateCategoryText()
    {
        _categoryText.text = Inventory.ItemCategories[SelectedCategory];
    }

    private void SetupCategorySelectionUI()
    {
        if (_categorySelectionUI == null)
        {
            _categorySelectionUI = gameObject.AddComponent<DummySelectionUI>();
            _categorySelectionUI.IgnoreVerticalInput = true;
            _categorySelectionUI.OnIndexChanged += OnCategoryChanged;
        }
        _categorySelectionUI.SetSelectionSettings(SelectionType.Grid, _availableCategoryIndices.Count);

        // Create dummy selectable items for each available category
        List<DummySelectable> categoryItems = new();
        for (int i = 0; i < _availableCategoryIndices.Count; i++)
        {
            categoryItems.Add(new DummySelectable());
        }
        _categorySelectionUI.SetItems(categoryItems);

        // Set the selected index based on current category
        int selIndex = _availableCategoryIndices.IndexOf(SelectedCategory);
        _categorySelectionUI.SetSelectedIndex(selIndex);
    }

    private void BuildItemList()
    {
        // Clear previous item slots
        foreach (Transform child in _itemList.transform)
        {
            Destroy(child.gameObject);
        }
        _slotUIList = new List<ItemSlotUI>();

        // Instantiate new item slot UI for each item in the selected category
        foreach (ItemSlot itemSlot in _inventory.GetSlotsByCategory(SelectedCategory))
        {
            ItemSlotUI slotUIObj = Instantiate(_itemSlotUI, _itemList.transform);
            slotUIObj.SetData(itemSlot);
            _slotUIList.Add(slotUIObj);
        }
    }

    public override void UpdateSelectionInUI()
    {
        List<ItemSlot> slots = _inventory.GetSlotsByCategory(SelectedCategory);
        if (slots.Count > 0)
        {
            ItemBase item = slots[_selectedItem].Item;
            _itemIcon.sprite = item.Icon;
            _itemDescription.text = item.Description;
        }
        HandleScrolling();
        base.UpdateSelectionInUI();
    }

    private void HandleScrolling()
    {
        if (_slotUIList.Count <= ITEMS_IN_VIEWPORT)
        {
            return;
        }

        int maxScrollIndex = _slotUIList.Count - ITEMS_IN_VIEWPORT;
        float scrollPos = Mathf.Clamp(_selectedItem - (ITEMS_IN_VIEWPORT / 2), 0, maxScrollIndex) * _slotUIList[0].Height;
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

    public override void ResetSelection()
    {
        _selectedItem = 0;
        _upArrow.gameObject.SetActive(false);
        _downArrow.gameObject.SetActive(false);
        _itemIcon.sprite = null;
        _itemDescription.text = "";
    }

    public override void EnableInput(bool enable)
    {
        base.EnableInput(enable);

        if (_categorySelectionUI != null)
        {
            _categorySelectionUI.EnableInput(enable);
        }
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

    private void OnCategoryChanged(int index)
    {
        // Update the current category based on the selection UI index
        SelectedCategory = _availableCategoryIndices[index];
        _categoryText.text = Inventory.ItemCategories[SelectedCategory];
        ResetSelection();
        UpdateCategoriesAndItemList();
    }

    public void ResetInventoryScreen()
    {
        SelectedCategory = GetFirstNonEmptyCategory();
        ResetSelection();
        UpdateCategoriesAndItemList();
    }
}