using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] private TextMeshProUGUI text;
    private Color originalColor;

    public void Init()
    {
        originalColor = GlobalSettings.Instance.InactiveColor;
    }

    public void Clear()
    {
        text.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        text.color = selected ? GlobalSettings.Instance.ActiveColor : originalColor;
    }

    public void SetText(string s)
    {
        text.text = s;
    }
}
