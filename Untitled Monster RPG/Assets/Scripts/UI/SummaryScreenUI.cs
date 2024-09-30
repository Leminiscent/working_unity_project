using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreenUI : MonoBehaviour
{
    [Header("Basic Details")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] Image image;

    [Header("Stats & Exp")]
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI spAttackText;
    [SerializeField] TextMeshProUGUI spDefenseText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] TextMeshProUGUI expToNextLevelText;
    [SerializeField] Transform expBar;
    Monster monster;

    public void SetBasicDetails(Monster monster)
    {
        this.monster = monster;

        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        image.sprite = monster.Base.Sprite;
    }

    public void SetStatsAndExp()
    {
        hpText.text = $"{monster.HP}/{monster.MaxHp}";
        attackText.text = "" + monster.Attack;
        defenseText.text = "" + monster.Defense;
        spAttackText.text = "" + monster.SpAttack;
        spDefenseText.text = "" + monster.SpDefense;
        speedText.text = "" + monster.Speed;

        expText.text = "" + monster.Exp;
        expToNextLevelText.text = "" + (monster.Base.GetExpForLevel(monster.Level + 1) - monster.Exp);
        expBar.localScale = new Vector2(monster.GetNormalizedExp(), 1);
    }
}
