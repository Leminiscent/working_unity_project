using TMPro;
using UnityEngine;

public class ItemEntryUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI itemQuantityText;

    public void SetItem(ItemBase item, int quantity)
    {
        itemNameText.text = item.Name;
        itemQuantityText.text = $"x{quantity}";
    }
}