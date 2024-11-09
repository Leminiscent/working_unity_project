using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _monsterTypeUI;
    [SerializeField] private TextMeshProUGUI _monsterType1;
    [SerializeField] private TextMeshProUGUI _monsterType2;

    [Header("Pages")]
    [SerializeField] private TextMeshProUGUI _pageNameText;
    [SerializeField] private GameObject _detailsPage;
    [SerializeField] private GameObject _movesPage;

    [Header("Stats & Exp")]
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _strengthText;
    [SerializeField] private TextMeshProUGUI _enduranceText;
    [SerializeField] private TextMeshProUGUI _intelligenceText;
    [SerializeField] private TextMeshProUGUI _fortitudeText;
    [SerializeField] private TextMeshProUGUI _agilityText;
    [SerializeField] private TextMeshProUGUI _expText;
    [SerializeField] private TextMeshProUGUI _expToNextLevelText;
    [SerializeField] private Transform _expBar;

    [Header("Moves")]
    [SerializeField] private List<TextMeshProUGUI> _moveTypes;
    [SerializeField] private List<TextMeshProUGUI> _moveNames;
    [SerializeField] private List<TextMeshProUGUI> _moveSP;
    [SerializeField] private TextMeshProUGUI _moveDescriptionText;
    [SerializeField] private TextMeshProUGUI _movePowerText;
    [SerializeField] private TextMeshProUGUI _moveAccuracyText;
    [SerializeField] private GameObject _moveEffectsUI;

    private List<TextSlot> _moveSlots;
    private Monster _monster;
    private bool _inMoveSelection;

    public bool InMoveSelection
    {
        get => _inMoveSelection;
        set
        {
            _inMoveSelection = value;
            if (_inMoveSelection)
            {
                _moveEffectsUI.SetActive(true);
                SetItems(_moveSlots.Take(_monster.Moves.Count).ToList());
            }
            else
            {
                _moveEffectsUI.SetActive(false);
                _moveDescriptionText.text = "";
                ClearItems();
            }
        }
    }

    private void Start()
    {
        _moveSlots = _moveNames.Select(static m => m.GetComponent<TextSlot>()).ToList();
        _moveEffectsUI.SetActive(false);
        _moveDescriptionText.text = "";
    }

    public void SetBasicDetails(Monster monster)
    {
        _monster = monster;

        _nameText.text = monster.Base.Name;
        _levelText.text = "Lvl " + monster.Level;
        _image.sprite = monster.Base.Sprite;
        
        _monsterType1.text = monster.Base.Type1.ToString().ToUpper();
        _monsterType2.text = monster.Base.Type2.ToString().ToUpper();
    }

    public void ShowPage(int index)
    {
        if (index == 0)
        {
            _pageNameText.text = "Monster Details";
            _detailsPage.SetActive(true);
            _monsterTypeUI.SetActive(true);
            _movesPage.SetActive(false);
            SetStatsAndExp();
        }
        else if (index == 1)
        {
            _pageNameText.text = "Monster Moves";
            _movesPage.SetActive(true);
            _detailsPage.SetActive(false);
            _monsterTypeUI.SetActive(false);
            SetMoves();
        }
    }

    public void SetStatsAndExp()
    {
        _hpText.text = $"{_monster.Hp}/{_monster.MaxHp}";
        _strengthText.text = "" + _monster.Strength;
        _enduranceText.text = "" + _monster.Endurance;
        _intelligenceText.text = "" + _monster.Intelligence;
        _fortitudeText.text = "" + _monster.Fortitude;
        _agilityText.text = "" + _monster.Agility;

        _expText.text = "" + _monster.Exp;

        if (_monster.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expToNextLevelText.text = "0";
        }
        else
        {
            int expToNextLevel = _monster.Base.GetExpForLevel(_monster.Level + 1) - _monster.Exp;

            _expToNextLevelText.text = "" + expToNextLevel;
        }

        _expBar.localScale = new Vector2(_monster.GetNormalizedExp(), 1);
    }

    public void SetMoves()
    {
        for (int i = 0; i < _moveNames.Count; i++)
        {
            if (i < _monster.Moves.Count)
            {
                Move move = _monster.Moves[i];

                _moveTypes[i].text = move.Base.Type.ToString().ToUpper();
                _moveNames[i].text = move.Base.Name;
                _moveSP[i].text = $"SP {move.Sp}/{move.Base.SP}";
            }
            else
            {
                _moveTypes[i].text = "-";
                _moveNames[i].text = "-";
                _moveSP[i].text = "-";
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

        Move move = _monster.Moves[_selectedItem];

        _moveDescriptionText.text = move.Base.Description;
        _movePowerText.text = move.Base.Power > 0 ? move.Base.Power.ToString() : "-";
        _moveAccuracyText.text = move.Base.Accuracy > 0 ? move.Base.Accuracy.ToString() : "-";
    }
}
