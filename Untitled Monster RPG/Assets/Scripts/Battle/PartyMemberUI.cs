using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private HPBar _hpBar;

    private Monster _monster;

    public void Init(Monster monster)
    {
        _monster = monster;
        UpdateData();
        SetMessage("");
        _monster.OnHPChanged += UpdateData;
    }

    private void UpdateData()
    {
        _nameText.text = _monster.Base.Name;
        _levelText.text = $"Lvl {_monster.Level}";
        _hpBar.SetHP((float)_monster.HP / _monster.MaxHP);
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
