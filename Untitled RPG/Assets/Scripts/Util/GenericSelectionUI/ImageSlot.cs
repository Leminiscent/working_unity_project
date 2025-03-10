using UnityEngine;
using UnityEngine.UI;

public class ImageSlot : MonoBehaviour, ISelectableItem
{
    private Image _bgImage;
    private Color _originalColor;

    private void Awake()
    {
        _bgImage = GetComponent<Image>();
    }

    public void Init()
    {
        _originalColor = _bgImage.color;
    }

    public void Clear()
    {
        _bgImage.color = _originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        _bgImage.color = selected ? GlobalSettings.Instance.BgHighlightColor : _originalColor;
    }
}
