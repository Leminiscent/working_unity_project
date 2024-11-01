using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;

    private int selectedCategory;
    private const int itemsInViewport = 8;
    private List<ItemSlotUI> slotUIList;
    private Inventory inventory;
    private RectTransform itemListRect;

    public ItemBase SelectedItem => inventory.GetItem(selectedItem, selectedCategory);
    public int SelectedCategory => selectedCategory;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        moneyText.text = $"{Wallet.Instance.Money} GP";
        selectedCategory = GetFirstNonEmptyCategory();
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
        Wallet.Instance.OnMoneyChanged += UpdateMoneyText;
    }

    private void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (ItemSlot itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            ItemSlotUI slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        SetItems(slotUIList.Select(static s => s.GetComponent<TextSlot>()).ToList());
        UpdateSelectionInUI();
    }

    private void UpdateMoneyText()
    {
        moneyText.text = $"{Wallet.Instance.Money} GP";
    }

    public override void HandleUpdate()
    {
        int prevCategory = selectedCategory;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedCategory = GetNextNonEmptyCategory(selectedCategory, 1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedCategory = GetNextNonEmptyCategory(selectedCategory, -1);
        }

        if (prevCategory != selectedCategory || Input.GetButtonDown("Back"))
        {
            if (Input.GetButtonDown("Back"))
            {
                selectedCategory = GetFirstNonEmptyCategory();
            }

            ResetSelction();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
            UpdateItemList();
        }

        base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        List<ItemSlot> slots = inventory.GetSlotsByCategory(selectedCategory);

        if (slots.Count > 0)
        {
            ItemBase item = slots[selectedItem].Item;

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();

        base.UpdateSelectionInUI();
    }

    private void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
        {
            return;
        }

        int maxScrollIndex = slotUIList.Count - itemsInViewport;
        float scrollPos = Mathf.Clamp(selectedItem - (itemsInViewport / 2), 0, maxScrollIndex) * slotUIList[0].Height;
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem < maxScrollIndex + (itemsInViewport / 2);

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    private void ResetSelction()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    private int GetNextNonEmptyCategory(int currentCategory, int direction)
    {
        int originalCategory = currentCategory;
        int categoryCount = Inventory.ItemCategories.Count;

        do
        {
            currentCategory = (currentCategory + direction + categoryCount) % categoryCount;

            List<ItemSlot> slots = inventory.GetSlotsByCategory(currentCategory);
            if (slots.Count > 0)
            {
                return currentCategory;
            }
        } while (currentCategory != originalCategory);

        return originalCategory; // Fallback
    }

    private int GetFirstNonEmptyCategory()
    {
        for (int i = 0; i < Inventory.ItemCategories.Count; i++)
        {
            List<ItemSlot> slots = inventory.GetSlotsByCategory(i);
            if (slots.Count > 0)
            {
                return i;
            }
        }

        return 0; // Fallback
    }
}