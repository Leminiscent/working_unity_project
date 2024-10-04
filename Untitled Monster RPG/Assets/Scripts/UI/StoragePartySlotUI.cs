using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoragePartySlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image image;

    public void SetData(Monster monster)
    {
        nameText.text = monster.Base.Name;
        levelText.text = "" + monster.Level;
        image.sprite = monster.Base.Sprite;
        image.color = new Color(255, 255, 255, 255);
    }

    public void ClearData()
    {
        nameText.text = "";
        levelText.text = "";
        image.sprite = null;
        image.color = new Color(255, 255, 255, 0);
    }
}
