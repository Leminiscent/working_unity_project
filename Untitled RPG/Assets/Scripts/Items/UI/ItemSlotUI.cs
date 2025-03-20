using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSlotUI : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_nameText")] public TextMeshProUGUI NameText { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_countText")] public TextMeshProUGUI CountText { get; private set; }

    private RectTransform _rectTransform;

    public float Height => _rectTransform.rect.height;

    public void SetData(object data)
    {
        _rectTransform = GetComponent<RectTransform>();

        // Inventory item slot
        if (data is ItemSlot itemSlot)
        {
            NameText.text = itemSlot.Item.Name;
            CountText.text = $"X {itemSlot.Count}";
        }
        // Shop item slot
        else if (data is ItemBase item)
        {
            NameText.text = item.Name;
            CountText.text = $"{item.Price} GP";
        }
        else
        {
            Debug.LogError("SetData received an unsupported type.");
        }
    }
}