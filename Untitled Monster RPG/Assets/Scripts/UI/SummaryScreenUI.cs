using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class SummaryScreenUI : SelectionUI<TextSlot>
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
    [SerializeField] TextMeshProUGUI strengthText;
    [SerializeField] TextMeshProUGUI enduranceText;
    [SerializeField] TextMeshProUGUI intelligenceText;
    [SerializeField] TextMeshProUGUI fortitudeText;
    [SerializeField] TextMeshProUGUI agilityText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] TextMeshProUGUI expToNextLevelText;
    [SerializeField] Transform expBar;

    [Header("Moves")]
    [SerializeField] List<TextMeshProUGUI> moveTypes;
    [SerializeField] List<TextMeshProUGUI> moveNames;
    [SerializeField] List<TextMeshProUGUI> moveSP;
    [SerializeField] TextMeshProUGUI moveDescriptionText;
    [SerializeField] TextMeshProUGUI movePowerText;
    [SerializeField] TextMeshProUGUI moveAccuracyText;
    [SerializeField] GameObject moveEffectsUI;

    List<TextSlot> moveSlots;
    Monster monster;
    bool inMoveSelection;

    public bool InMoveSelection
    {
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;
            if (inMoveSelection)
            {
                moveEffectsUI.SetActive(true);
                SetItems(moveSlots.Take(monster.Moves.Count).ToList());
            }
            else
            {
                moveEffectsUI.SetActive(false);
                moveDescriptionText.text = "";
                ClearItems();
            }
        }
    }

    private void Start()
    {
        moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
        moveEffectsUI.SetActive(false);
        moveDescriptionText.text = "";
    }

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
            pageNameText.text = "Monster Stats";
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
        hpText.text = $"{monster.HP}/{monster.MaxHP}";
        strengthText.text = "" + monster.Strength;
        enduranceText.text = "" + monster.Endurance;
        intelligenceText.text = "" + monster.Intelligence;
        fortitudeText.text = "" + monster.Fortitude;
        agilityText.text = "" + monster.Agility;

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

    public override void HandleUpdate()
    {
        if (InMoveSelection)
        {
            base.HandleUpdate();
        }
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        var move = monster.Moves[selectedItem];

        moveDescriptionText.text = move.Base.Description;
        movePowerText.text = move.Base.Power > 0 ? move.Base.Power.ToString() : "-";
        moveAccuracyText.text = move.Base.Accuracy > 0 ? move.Base.Accuracy.ToString() : "-";
    }
}
