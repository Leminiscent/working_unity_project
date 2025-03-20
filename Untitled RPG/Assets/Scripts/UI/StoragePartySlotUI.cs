using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoragePartySlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _roleText;
    [SerializeField] private Image _image;

    public void SetData(Battler battler)
    {
        if (battler == null)
        {
            ClearData();
            return;
        }

        BattlerBase battlerBase = battler.Base;
        _nameText.text = battlerBase.Name;
        _levelText.text = $"Lvl {battler.Level}";
        _roleText.text = battler.IsCommander ? "Commander" : string.Empty;
        _image.sprite = battlerBase.Sprite;
        _image.color = new Color(1, 1, 1, 1);
    }

    public void ClearData()
    {
        _nameText.text = "";
        _levelText.text = "";
        _roleText.text = "";
        _image.sprite = null;
        _image.color = new Color(1, 1, 1, 0);
    }
}