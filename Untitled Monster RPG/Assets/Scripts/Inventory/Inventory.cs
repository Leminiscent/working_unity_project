using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory
{
    RecoveryItems,
    MonsterParts,
    SkillBooks
}

public class Inventory : MonoBehaviour, ISavable
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

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot { Item = item, Count = count });
        }

        OnUpdated?.Invoke();
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

    ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
        {
            return ItemCategory.RecoveryItems;
        }
        else if (item is MonsterPart)
        {
            return ItemCategory.MonsterParts;
        }
        else
        {
            return ItemCategory.SkillBooks;
        }
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData
        {
            recoveryItems = recoveryItemSlots.Select(slot => slot.GetSaveData()).ToList(),
            monsterParts = monsterPartSlots.Select(slot => slot.GetSaveData()).ToList(),
            skillBooks = skillBookSlots.Select(slot => slot.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        recoveryItemSlots = saveData.recoveryItems.Select(data => new ItemSlot(data)).ToList();
        monsterPartSlots = saveData.monsterParts.Select(data => new ItemSlot(data)).ToList();
        skillBookSlots = saveData.skillBooks.Select(data => new ItemSlot(data)).ToList();

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot() { }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetItemByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData
        {
            name = item.Name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item { get => item; set => item = value; }
    public int Count { get => count; set => count = value; }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> recoveryItems;
    public List<ItemSaveData> monsterParts;
    public List<ItemSaveData> skillBooks;
}