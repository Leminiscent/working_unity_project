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

    [Header("Pages")]
    [SerializeField] TextMeshProUGUI pageNameText;
    [SerializeField] GameObject skillsPage;
    [SerializeField] GameObject movesPage;

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

    [Header("Moves")]
    [SerializeField] List<TextMeshProUGUI> moveTypes;
    [SerializeField] List<TextMeshProUGUI> moveNames;
    [SerializeField] List<TextMeshProUGUI> moveSP;

    Monster monster;

    public void SetBasicDetails(Monster monster)
    {
        this.monster = monster;

        nameText.text = monster.Base.Name;
        levelText.text = "Lvl " + monster.Level;
        image.sprite = monster.Base.Sprite;
    }

    public void ShowPage(int index)
    {
        if (index == 0)
        {
            pageNameText.text = "Monster Skills";
            skillsPage.SetActive(true);
            movesPage.SetActive(false);
            SetStatsAndExp();
        }
        else if (index == 1)
        {
            pageNameText.text = "Monster Moves";
            movesPage.SetActive(true);
            skillsPage.SetActive(false);
            SetMoves();
        }
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

    public void SetMoves()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < monster.Moves.Count)
            {
                var move = monster.Moves[i];

                moveTypes[i].text = move.Base.Type.ToString().ToUpper();
                moveNames[i].text = move.Base.Name;
                moveSP[i].text = $"SP {move.SP}/{move.Base.SP}";
            }
            else
            {
                moveTypes[i].text = "-";
                moveNames[i].text = "-";
                moveSP[i].text = "-";
            }
        }
    }
}
