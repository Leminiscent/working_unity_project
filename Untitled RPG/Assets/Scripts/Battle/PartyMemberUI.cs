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
        if (_battler != null)
        {
            _battler.OnHPChanged += UpdateData;
        }
    }

    private void UpdateData()
    {
        if (_battler == null)
        {
            Debug.LogWarning("Battler data is missing in PartyMemberUI.UpdateData.");
            return;
        }

        if (_nameText != null)
        {
            _nameText.text = _battler.Base.Name;
        }

        if (_levelText != null)
        {
            _levelText.text = $"Lvl {_battler.Level}";
        }

        if (_hpBar != null)
        {
            _hpBar.SetHP((float)_battler.Hp / _battler.MaxHp);
        }
    }

    public void SetSelected(bool selected)
    {
        if (_nameText != null && GlobalSettings.Instance != null)
        {
            _nameText.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }
    }

    public void SetMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }
}
