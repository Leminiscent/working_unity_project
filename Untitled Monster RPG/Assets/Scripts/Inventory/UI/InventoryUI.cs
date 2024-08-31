using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;
    Action<ItemBase> OnItemUsed;
    int selectedItem = 0;
    int selectedCategory = 0;
    InventoryUIState state;
    const int itemsInViewport = 8;
    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.OnItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++selectedCategory;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --selectedCategory;
            }

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
            {
                selectedCategory = 0;
            }
            else if (selectedCategory < 0)
            {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (prevCategory != selectedCategory)
            {
                ResetSelction();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                ItemSelected();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }

    void ItemSelected()
    {
        OpenPartyScreen();
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;

        var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);

        if (usedItem != null)
        {
            yield return DialogueManager.Instance.ShowDialogueText($"The {usedItem.Name} was used on {partyScreen.SelectedMember.Base.Name}!");
            yield return DialogueManager.Instance.ShowDialogueText($"{partyScreen.SelectedMember.Base.Name} {usedItem.Message}!");
            OnItemUsed?.Invoke(usedItem);
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {partyScreen.SelectedMember.Base.Name}!");
        }
        ClosePartyScreen();
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.Instance.InactiveColor;
            }
        }

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        int maxScrollIndex = slotUIList.Count - itemsInViewport;
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, maxScrollIndex) * slotUIList[0].Height;
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem < maxScrollIndex + itemsInViewport / 2;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelction()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
    }
}