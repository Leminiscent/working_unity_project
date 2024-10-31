using UnityEngine;
using UnityEngine.UI;

public class StorageSlotUI : MonoBehaviour
{
    [SerializeField] private Image image;

    public void SetData(Monster monster)
    {
        image.sprite = monster.Base.Sprite;
        image.color = new Color(1, 1, 1, 1);
    }

    public void ClearData()
    {
        image.sprite = null;
        image.color = new Color(1, 1, 1, 0);
    }
}
