using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private HPBar hpBar;
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
        nameText.text = _monster.Base.Name;
        levelText.text = $"Lvl {_monster.Level}";
        hpBar.SetHP((float)_monster.HP / _monster.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        nameText.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
