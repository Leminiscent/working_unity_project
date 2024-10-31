using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject itemList;
    [SerializeField] private ItemSlotUI itemSlotUI;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemDescription;
    [SerializeField] private Image upArrow;
    [SerializeField] private Image downArrow;
    private int selectedItem;
    private List<ItemBase> availableItems;
    private Action<ItemBase> onItemSelected;
    private Action onBack;
    private List<ItemSlotUI> slotUIList;
    private const int itemsInViewport = 8;
    private RectTransform itemListRect;

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
        int prevSelection = selectedItem;

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

    private void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (ItemBase item in availableItems)
        {
            ItemSlotUI slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetNameAndPrice(item);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    private void UpdateItemSelection()
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            slotUIList[i].NameText.color = i == selectedItem ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }

        if (availableItems.Count > 0)
        {
            ItemBase item = availableItems[selectedItem];

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
        {
            return;
        }

        int maxScrollIndex = slotUIList.Count - itemsInViewport;
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, maxScrollIndex) * slotUIList[0].Height;
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem < maxScrollIndex + itemsInViewport / 2;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
