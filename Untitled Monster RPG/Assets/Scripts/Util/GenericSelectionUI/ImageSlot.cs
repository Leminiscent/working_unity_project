using UnityEngine;
using UnityEngine.UI;

public class ImageSlot : MonoBehaviour, ISelectableItem
{
    Image bgImage;
    Color originalColor;

    private void Awake()
    {
        bgImage = GetComponent<Image>();
    }

    public void Init()
    {
        originalColor = bgImage.color;
    }

    public void Clear()
    {
        bgImage.color = originalColor;
    }

    public void OnSelectionChanged(bool selected)
    {
        bgImage.color = selected ? GlobalSettings.Instance.BgHighlightColor : originalColor;
    }
}
