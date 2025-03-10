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
    [SerializeField] private GameObject _battlerTypeUI;
    [SerializeField] private TextMeshProUGUI _battlerType1;
    [SerializeField] private TextMeshProUGUI _battlerType2;

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
    [SerializeField] private GameObject _expBar;

    [Header("Moves")]
    [SerializeField] private List<TextMeshProUGUI> _moveTypes;
    [SerializeField] private List<TextMeshProUGUI> _moveNames;
    [SerializeField] private List<TextMeshProUGUI> _moveSP;
    [SerializeField] private TextMeshProUGUI _moveDescriptionText;
    [SerializeField] private TextMeshProUGUI _movePowerText;
    [SerializeField] private TextMeshProUGUI _moveAccuracyText;
    [SerializeField] private GameObject _moveEffectsUI;

    private List<TextSlot> _moveSlots;
    private Battler _battler;
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
                SetItems(_moveSlots.Take(_battler.Moves.Count).ToList());
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

    public void SetBasicDetails(Battler battler)
    {
        _battler = battler;

        _nameText.text = battler.Base.Name;
        _levelText.text = "Lvl " + battler.Level;
        _image.sprite = battler.Base.Sprite;

        _battlerType1.text = battler.Base.Type1.ToString().ToUpper();
        if (battler.Base.Type2 != BattlerType.None)
        {
            _battlerType2.transform.parent.gameObject.SetActive(true);
            _battlerType2.text = battler.Base.Type2.ToString().ToUpper();
        }
        else
        {
            _battlerType2.transform.parent.gameObject.SetActive(false);
        }
    }

    public void ShowPage(int index)
    {
        if (index == 0)
        {
            _pageNameText.text = "Battler Details";
            _detailsPage.SetActive(true);
            _battlerTypeUI.SetActive(true);
            _movesPage.SetActive(false);
            SetStatsAndExp();
        }
        else if (index == 1)
        {
            _pageNameText.text = "Battler Moves";
            _movesPage.SetActive(true);
            _detailsPage.SetActive(false);
            _battlerTypeUI.SetActive(false);
            SetMoves();
        }
    }

    public void SetStatsAndExp()
    {
        _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
        _strengthText.text = "" + _battler.Strength;
        _enduranceText.text = "" + _battler.Endurance;
        _intelligenceText.text = "" + _battler.Intelligence;
        _fortitudeText.text = "" + _battler.Fortitude;
        _agilityText.text = "" + _battler.Agility;

        _expText.text = "" + _battler.Exp;

        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expToNextLevelText.text = "Max Level";
        }
        else
        {
            int expToNextLevel = _battler.Base.GetExpForLevel(_battler.Level + 1) - _battler.Exp;

            _expToNextLevelText.text = "" + expToNextLevel;
        }

        _expBar.transform.localScale = new Vector3(_battler.GetNormalizedExp(), 1, 1);
    }

    public void SetMoves()
    {
        for (int i = 0; i < _moveNames.Count; i++)
        {
            if (i < _battler.Moves.Count)
            {
                Move move = _battler.Moves[i];

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

        Move move = _battler.Moves[_selectedItem];

        _moveDescriptionText.text = move.Base.Description;
        _movePowerText.text = move.Base.Power > 0 ? move.Base.Power.ToString() : "-";
        _moveAccuracyText.text = move.Base.Accuracy > 0 ? move.Base.Accuracy.ToString() : "-";
    }
}
