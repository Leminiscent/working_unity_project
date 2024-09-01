using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory
{
    RecoveryItems,
    MonsterParts,
    Scrolls,
    SkillBooks
}

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> recoveryItemSlots;
    [SerializeField] List<ItemSlot> monsterPartSlots;
    [SerializeField] List<ItemSlot> skillBookSlots;
    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    public void Awake()
    {
        allSlots = new List<List<ItemSlot>>()
        {
            recoveryItemSlots,
            monsterPartSlots,
            skillBookSlots
        };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "RECOVERY ITEMS",
        "MONSTER PARTS",
        "SKILL BOOKS"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlots = GetSlotsByCategory(categoryIndex);

        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Monster selectedMonster, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedMonster);

        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item, selectedCategory);
            }
            return item;
        }
        else
        {
            return null;
        }
    }

    public void RemoveItem(ItemBase item, int categoryIndex)
    {
        var currentSlots = GetSlotsByCategory(categoryIndex);
        var itemSlot = currentSlots.First(slot => slot.Item == item);

        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            currentSlots.Remove(itemSlot);
        }
        OnUpdated?.Invoke();
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[System.Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;
    public int Count { get => count; set => count = value; }
}