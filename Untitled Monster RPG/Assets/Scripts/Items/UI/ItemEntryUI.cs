using TMPro;
using UnityEngine;

public class ItemEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;

    public void SetItem(ItemBase item, int quantity)
    {
        itemNameText.text = item.Name;
        itemQuantityText.text = $"x{quantity}";
    }
}