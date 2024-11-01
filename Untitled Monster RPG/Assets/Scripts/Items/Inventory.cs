using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory
{
    RecoveryItems,
    Materials,
    TransformationItems,
    SkillBooks,
    KeyItems
}

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] private List<ItemSlot> recoveryItemSlots;
    [SerializeField] private List<ItemSlot> materialSlots;
    [SerializeField] private List<ItemSlot> transformationItemSlots;
    [SerializeField] private List<ItemSlot> skillBookSlots;
    [SerializeField] private List<ItemSlot> keyItemSlots;
    private List<List<ItemSlot>> allSlots;

    public event Action OnUpdated;

    public void Awake()
    {
        allSlots = new List<List<ItemSlot>>()
        {
            recoveryItemSlots,
            materialSlots,
            transformationItemSlots,
            skillBookSlots,
            keyItemSlots
        };
    }

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "RECOVERY ITEMS",
        "MATERIALS",
        "TRANSFORMATION ITEMS",
        "SKILL BOOKS",
        "KEY ITEMS"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        List<ItemSlot> currentSlots = GetSlotsByCategory(categoryIndex);

        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Monster selectedMonster, int selectedCategory)
    {
        ItemBase item = GetItem(itemIndex, selectedCategory);

        return UseItem(item, selectedMonster);
    }

    public ItemBase UseItem(ItemBase item, Monster selectedMonster)
    {
        bool itemUsed = item.Use(selectedMonster);

        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item);
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
        List<ItemSlot> currentSlots = GetSlotsByCategory(category);
        ItemSlot itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

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

    public int GetItemCount(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        List<ItemSlot> currentSlots = GetSlotsByCategory(category);
        ItemSlot itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);

        return itemSlot != null ? itemSlot.Count : 0;
    }

    public void RemoveItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCategoryFromItem(item);
        List<ItemSlot> currentSlots = GetSlotsByCategory(category);
        ItemSlot itemSlot = currentSlots.First(slot => slot.Item == item);

        itemSlot.Count -= count;
        if (itemSlot.Count == 0)
        {
            currentSlots.Remove(itemSlot);
        }
        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCategoryFromItem(item);
        List<ItemSlot> currentSlots = GetSlotsByCategory(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    private ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
        {
            return ItemCategory.RecoveryItems;
        }
        else if (item is Material)
        {
            return ItemCategory.Materials;
        }
        else if (item is TransformationItem)
        {
            return ItemCategory.TransformationItems;
        }
        else
        {
            return item is SkillBook ? ItemCategory.SkillBooks : ItemCategory.KeyItems;
        }
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        InventorySaveData saveData = new()
        {
            recoveryItems = recoveryItemSlots.Select(static slot => slot.GetSaveData()).ToList(),
            materials = materialSlots.Select(static slot => slot.GetSaveData()).ToList(),
            transformationItems = transformationItemSlots.Select(static slot => slot.GetSaveData()).ToList(),
            skillBooks = skillBookSlots.Select(static slot => slot.GetSaveData()).ToList(),
            keyItems = keyItemSlots.Select(static slot => slot.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        InventorySaveData saveData = state as InventorySaveData;

        recoveryItemSlots = saveData.recoveryItems.Select(static data => new ItemSlot(data)).ToList();
        materialSlots = saveData.materials.Select(static data => new ItemSlot(data)).ToList();
        transformationItemSlots = saveData.transformationItems.Select(static data => new ItemSlot(data)).ToList();
        skillBookSlots = saveData.skillBooks.Select(static data => new ItemSlot(data)).ToList();
        keyItemSlots = saveData.keyItems.Select(static data => new ItemSlot(data)).ToList();

        allSlots = new List<List<ItemSlot>>()
        {
            recoveryItemSlots,
            materialSlots,
            transformationItemSlots,
            skillBookSlots,
            keyItemSlots
        };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count;

    public ItemSlot() { }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        ItemSaveData saveData = new()
        {
            name = item.name,
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
    public List<ItemSaveData> materials;
    public List<ItemSaveData> transformationItems;
    public List<ItemSaveData> skillBooks;
    public List<ItemSaveData> keyItems;
}