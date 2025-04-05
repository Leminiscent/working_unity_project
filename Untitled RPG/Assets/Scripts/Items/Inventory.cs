using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] private List<ItemSlot> _recoveryItemSlots;
    [SerializeField] private List<ItemSlot> _ingredientSlots;
    [SerializeField] private List<ItemSlot> _transformationItemSlots;
    [SerializeField] private List<ItemSlot> _skillBookSlots;
    [SerializeField] private List<ItemSlot> _keyItemSlots;

    private List<List<ItemSlot>> _allSlots;

    public event Action OnUpdated;

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "RECOVERY ITEMS",
        "INGREDIENTS",
        "TRANSFORMATION ITEMS",
        "SKILL BOOKS",
        "KEY ITEMS"
    };

    public void Awake()
    {
        _allSlots = new List<List<ItemSlot>>()
        {
            _recoveryItemSlots,
            _ingredientSlots,
            _transformationItemSlots,
            _skillBookSlots,
            _keyItemSlots
        };
    }

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return _allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        List<ItemSlot> currentSlots = GetSlotsByCategory(categoryIndex);
        return currentSlots[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Battler selectedBattler, int selectedCategory)
    {
        ItemBase item = GetItem(itemIndex, selectedCategory);
        return UseItem(item, selectedBattler);
    }

    public ItemBase UseItem(ItemBase item, Battler selectedBattler)
    {
        bool itemUsed = item.Use(selectedBattler);
        if (itemUsed)
        {
            if (!item.IsReusable)
            {
                RemoveItem(item);
            }
            return item;
        }
        return null;
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
            _ = currentSlots.Remove(itemSlot);
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
        return item switch
        {
            RecoveryItem => ItemCategory.RecoveryItems,
            Ingredient => ItemCategory.Ingredients,
            TransformationItem => ItemCategory.TransformationItems,
            SkillBook => ItemCategory.SkillBooks,
            _ => ItemCategory.KeyItems,
        };
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        InventorySaveData saveData = new()
        {
            RecoveryItems = _recoveryItemSlots.Select(static slot => slot.GetSaveData()).ToList(),
            Ingredients = _ingredientSlots.Select(static slot => slot.GetSaveData()).ToList(),
            TransformationItems = _transformationItemSlots.Select(static slot => slot.GetSaveData()).ToList(),
            SkillBooks = _skillBookSlots.Select(static slot => slot.GetSaveData()).ToList(),
            KeyItems = _keyItemSlots.Select(static slot => slot.GetSaveData()).ToList()
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        InventorySaveData saveData = state as InventorySaveData;

        _recoveryItemSlots = saveData.RecoveryItems.Select(static data => new ItemSlot(data)).ToList();
        _ingredientSlots = saveData.Ingredients.Select(static data => new ItemSlot(data)).ToList();
        _transformationItemSlots = saveData.TransformationItems.Select(static data => new ItemSlot(data)).ToList();
        _skillBookSlots = saveData.SkillBooks.Select(static data => new ItemSlot(data)).ToList();
        _keyItemSlots = saveData.KeyItems.Select(static data => new ItemSlot(data)).ToList();

        _allSlots = new List<List<ItemSlot>>()
        {
            _recoveryItemSlots,
            _ingredientSlots,
            _transformationItemSlots,
            _skillBookSlots,
            _keyItemSlots
        };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private int _count;

    public ItemBase Item { get => _item; set => _item = value; }
    public int Count { get => _count; set => _count = value; }

    public ItemSlot() { }

    public ItemSlot(ItemSaveData saveData)
    {
        _item = ItemDB.GetObjectByName(saveData.Name);
        _count = saveData.Count;
    }

    public ItemSaveData GetSaveData()
    {
        return new ItemSaveData
        {
            Name = _item.name,
            Count = _count
        };
    }
}

public enum ItemCategory
{
    RecoveryItems,
    Ingredients,
    TransformationItems,
    SkillBooks,
    KeyItems
}

[Serializable]
public class ItemSaveData
{
    public string Name;
    public int Count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> RecoveryItems;
    public List<ItemSaveData> Ingredients;
    public List<ItemSaveData> TransformationItems;
    public List<ItemSaveData> SkillBooks;
    public List<ItemSaveData> KeyItems;
}