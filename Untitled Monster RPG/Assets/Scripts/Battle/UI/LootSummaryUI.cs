using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LootSummaryUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] GameObject lootListContainer;
    [SerializeField] ItemSlotUI itemSlotUI;

    public void DisplayGold(int gold)
    {
        if (gold == 0)
        {
            goldText.text = "";
        }
        else
        {
            goldText.text = $"{gold} GP";
        }
    }

    public void DisplayItems(Dictionary<ItemBase, int> items)
    {
        foreach (Transform child in lootListContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in items)
        {
            var lootItem = Instantiate(itemSlotUI, lootListContainer.transform);

            lootItem.SetData(new ItemSlot { Item = item.Key, Count = item.Value });
        }
    }
}
