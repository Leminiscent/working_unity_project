using TMPro;
using UnityEngine;

public class ChoiceText : MonoBehaviour
{
    TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void SetSelected(bool selected)
    {
        text.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
    }

    public TextMeshProUGUI TextField => text;
}
