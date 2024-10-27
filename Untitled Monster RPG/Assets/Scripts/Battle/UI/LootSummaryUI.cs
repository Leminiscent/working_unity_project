using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LootSummaryUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] Transform itemList;
    [SerializeField] GameObject itemEntryPrefab;

    public void DisplayGold(int gold)
    {
        if (gold == 0)
        {
            goldText.gameObject.SetActive(false);
        }
        else
        {
            goldText.text = $"{gold} GP";
        }
    }

    public void DisplayItems(Dictionary<ItemBase, int> items)
    {
        foreach (Transform child in itemList)
        {
            Destroy(child.gameObject);
        }

        if (items.Count == 0)
        {
            return;
        }

        foreach (var item in items)
        {
            GameObject itemEntryObj = Instantiate(itemEntryPrefab, itemList);
            ItemEntryUI itemEntryUI = itemEntryObj.GetComponent<ItemEntryUI>();
            
            itemEntryUI.SetItem(item.Key, item.Value);
        }
    }
}