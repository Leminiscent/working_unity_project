using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageSlotUI : MonoBehaviour
{
    [SerializeField] Image image;

    public void SetData(Monster monster)
    {
        image.sprite = monster.Base.Sprite;
        image.color = new Color(255, 255, 255, 255);
    }

    public void ClearData()
    {
        image.sprite = null;
        image.color = new Color(255, 255, 255, 0);
    }
}
