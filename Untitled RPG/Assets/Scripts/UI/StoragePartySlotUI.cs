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
        _nameText.text = battler.Base.Name;
        _levelText.text = "Lvl " + battler.Level;
        _roleText.text = battler.IsCommander ? "Commander" : "";
        _image.sprite = battler.Base.Sprite;
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
