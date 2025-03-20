using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] private TextMeshProUGUI _text;

    private Color _originalColor;

    public void Init()
    {
        _originalColor = GlobalSettings.Instance.InactiveColor;
    }

    public void Clear()
    {
        _text.color = _originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        _text.color = selected ? GlobalSettings.Instance.ActiveColor : _originalColor;
    }

    public void SetText(string s)
    {
        _text.text = s;
    }
}