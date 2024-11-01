using UnityEngine;
using UnityEngine.UI;

public class StorageSlotUI : MonoBehaviour
{
    [SerializeField] private Image _image;

    public void SetData(Monster monster)
    {
        _image.sprite = monster.Base.Sprite;
        _image.color = new Color(1, 1, 1, 1);
    }

    public void ClearData()
    {
        _image.sprite = null;
        _image.color = new Color(1, 1, 1, 0);
    }
}
