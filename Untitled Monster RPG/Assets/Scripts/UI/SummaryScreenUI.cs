using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image image;

    [Header("Pages")]
    [SerializeField] private TextMeshProUGUI pageNameText;
    [SerializeField] private GameObject detailsPage;
    [SerializeField] private GameObject movesPage;

    [Header("Stats & Exp")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI enduranceText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI fortitudeText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private TextMeshProUGUI expToNextLevelText;
    [SerializeField] private Transform expBar;

    [Header("Moves")]
    [SerializeField] private List<TextMeshProUGUI> moveTypes;
    [SerializeField] private List<TextMeshProUGUI> moveNames;
    [SerializeField] private List<TextMeshProUGUI> moveSP;
    [SerializeField] private TextMeshProUGUI moveDescriptionText;
    [SerializeField] private TextMeshProUGUI movePowerText;
    [SerializeField] private TextMeshProUGUI moveAccuracyText;
    [SerializeField] private GameObject moveEffectsUI;
    private List<TextSlot> moveSlots;
    private Monster monster;
    private bool inMoveSelection;

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
        moveSlots = moveNames.Select(static m => m.GetComponent<TextSlot>()).ToList();
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
            pageNameText.text = "Monster Details";
            detailsPage.SetActive(true);
            movesPage.SetActive(false);
            SetStatsAndExp();
        }
        else if (index == 1)
        {
            pageNameText.text = "Monster Moves";
            movesPage.SetActive(true);
            detailsPage.SetActive(false);
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

        if (monster.Level == GlobalSettings.Instance.MaxLevel)
        {
            expToNextLevelText.text = "0";
        }
        else
        {
            int expToNextLevel = monster.Base.GetExpForLevel(monster.Level + 1) - monster.Exp;

            expToNextLevelText.text = "" + expToNextLevel;
        }

        expBar.localScale = new Vector2(monster.GetNormalizedExp(), 1);
    }

    public void SetMoves()
    {
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < monster.Moves.Count)
            {
                Move move = monster.Moves[i];

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

        Move move = monster.Moves[selectedItem];

        moveDescriptionText.text = move.Base.Description;
        movePowerText.text = move.Base.Power > 0 ? move.Base.Power.ToString() : "-";
        moveAccuracyText.text = move.Base.Accuracy > 0 ? move.Base.Accuracy.ToString() : "-";
    }
}
