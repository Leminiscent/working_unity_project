using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] TextMeshProUGUI text;
    Color originalColor;

    public void Init()
    {
        originalColor = GlobalSettings.Instance.InactiveColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        text.color = selected ? GlobalSettings.Instance.ActiveColor : originalColor;
    }
}
