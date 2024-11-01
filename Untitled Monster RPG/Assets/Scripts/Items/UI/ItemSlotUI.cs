using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _countText;

    private RectTransform _rectTransform;

    public TextMeshProUGUI NameText => _nameText;
    public TextMeshProUGUI CountText => _countText;
    public float Height => _rectTransform.rect.height;

    public void SetData(ItemSlot itemSlot)
    {
        _rectTransform = GetComponent<RectTransform>();
        _nameText.text = itemSlot.Item.Name;
        _countText.text = $"X {itemSlot.Count}";
    }

    public void SetNameAndPrice(ItemBase item)
    {
        _rectTransform = GetComponent<RectTransform>();
        _nameText.text = item.Name;
        _countText.text = $"{item.Price} GP";
    }
}
