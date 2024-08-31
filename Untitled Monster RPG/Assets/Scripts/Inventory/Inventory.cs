using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> monsterPartSlots;
    [SerializeField] List<ItemSlot> skillBookSlots;
    List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    public void Awake()
    {
        allSlots = new List<List<ItemSlot>>()
        {
            slots,
            monsterPartSlots,
            skillBookSlots
        };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS",
        "MONSTER PARTS",
        "SKILL BOOKS"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase UseItem(int itemIndex, Monster selectedMonster)
    {
        var item = slots[itemIndex].Item;
        bool itemUsed = item.Use(selectedMonster);

        if (itemUsed)
        {
            RemoveItem(item);
            return item;
        }
        else
        {
            return null;
        }
    }

    public void RemoveItem(ItemBase item)
    {
        var itemSlot = slots.First(slot => slot.Item == item);

        itemSlot.Count--;
        if (itemSlot.Count == 0)
        {
            slots.Remove(itemSlot);
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