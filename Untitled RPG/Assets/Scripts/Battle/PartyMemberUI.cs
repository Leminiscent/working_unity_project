using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private HPBar _hpBar;

    private Battler _battler;

    public void Init(Battler battler)
    {
        _battler = battler;
        UpdateData();
        SetMessage("");
        _battler.OnHPChanged += UpdateData;
    }

    private void UpdateData()
    {
        _nameText.text = _battler.Base.Name;
        _levelText.text = $"Lvl {_battler.Level}";
        _hpBar.SetHP((float)_battler.Hp / _battler.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        _nameText.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
    }

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }
}
