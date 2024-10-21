using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    int selectedItem;
    List<ItemBase> availableItems;
    Action<ItemBase> onItemSelected;
    Action onBack;
    List<ItemSlotUI> slotUIList;
    const int itemsInViewport = 8;
    RectTransform itemListRect;

    private void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        var prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++selectedItem;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --selectedItem;
        }
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);
        if (prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onItemSelected?.Invoke(availableItems[selectedItem]);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            selectedItem = 0;
            onBack?.Invoke();
        }
    }

    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetNameAndPrice(item);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);
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

        if (availableItems.Count > 0)
        {
            var item = availableItems[selectedItem];

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
}
