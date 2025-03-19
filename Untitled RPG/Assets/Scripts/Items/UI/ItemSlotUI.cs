using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ItemSlotUI : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_nameText")] public TextMeshProUGUI NameText { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_countText")] public TextMeshProUGUI CountText { get; private set; }

    private RectTransform _rectTransform;

    public float Height => _rectTransform.rect.height;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void SetData(ItemSlot itemSlot)
    {
        NameText.text = itemSlot.Item.Name;
        CountText.text = $"X {itemSlot.Count}";
    }

    public void SetNameAndPrice(ItemBase item)
    {
        NameText.text = item.Name;
        CountText.text = $"{item.Price} GP";
    }
}