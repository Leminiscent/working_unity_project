using TMPro;
using UnityEngine;

public class ChoiceText : MonoBehaviour
{
    private TextMeshProUGUI _text;

    public TextMeshProUGUI TextField => _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void SetSelected(bool selected)
    {
        _text.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
    }
}
