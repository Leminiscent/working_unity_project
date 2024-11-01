using TMPro;
using UnityEngine;

public class ItemEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemQuantityText;

    public void SetItem(ItemBase item, int quantity)
    {
        _itemNameText.text = item.Name;
        _itemQuantityText.text = $"x{quantity}";
    }
}