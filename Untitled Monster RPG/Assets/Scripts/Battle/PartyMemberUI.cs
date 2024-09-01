using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] HPBar hpBar;

    Monster _monster;

    public void Init(Monster monster)
    {
        _monster = monster;
        UpdateData();
        SetMessage("");
        _monster.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _monster.Base.Name;
        levelText.text = $"Lvl {_monster.Level}";
        hpBar.SetHP((float)_monster.HP / _monster.MaxHp);
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
