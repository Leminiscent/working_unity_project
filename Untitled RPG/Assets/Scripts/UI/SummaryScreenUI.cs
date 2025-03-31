using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util.GenericSelectionUI;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _battlerTypeUI;
    [SerializeField] private TextMeshProUGUI _battlerType;

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
    [SerializeField] private GameObject _moveDescriptionUI;
    [SerializeField] private TextMeshProUGUI _moveDescriptionText;
    [SerializeField] private GameObject _moveEffectsUI;
    [SerializeField] private TextMeshProUGUI _movePowerText;
    [SerializeField] private TextMeshProUGUI _moveAccuracyText;

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
                _moveDescriptionUI.SetActive(true);
                _moveEffectsUI.SetActive(true);
                // Only set as many move slots as there are moves.
                SetItems(_moveSlots.Take(_battler.Moves.Count).ToList());
            }
            else
            {
                _moveDescriptionUI.SetActive(false);
                _moveEffectsUI.SetActive(false);
                _moveDescriptionText.text = string.Empty;
                ClearItems();
            }
        }
    }

    private void Start()
    {
        // Cache the TextSlot components from the move names.
        _moveSlots = _moveNames.Select(static m => m.GetComponent<TextSlot>()).ToList();
        _moveDescriptionUI.SetActive(false);
        _moveEffectsUI.SetActive(false);
        _moveDescriptionText.text = string.Empty;
    }

    public void SetBasicDetails(Battler battler)
    {
        _battler = battler;
        BattlerBase battlerBase = battler.Base;

        _nameText.text = battlerBase.Name;
        _levelText.text = $"Lvl {battler.Level}";
        _image.sprite = battlerBase.Sprite;

        _battlerType.text = $"{battlerBase.Type1}{(battlerBase.Type2 != BattlerType.None ? $" / {battlerBase.Type2}" : "")}";
    }

    public void ShowPage(int index)
    {
        switch (index) // TODO: Tween pages left and right.
        {
            case 0:
                _pageNameText.text = "Battler Details";
                _detailsPage.SetActive(true);
                _movesPage.SetActive(false);
                _battlerTypeUI.SetActive(true);
                SetStatsAndExp();
                break;
            case 1:
                _pageNameText.text = "Battler Moves";
                _movesPage.SetActive(true);
                _detailsPage.SetActive(false);
                _battlerTypeUI.SetActive(false);
                SetMoves();
                break;
            default:
                Debug.LogWarning("Invalid page index.");
                break;
        }
    }

    public void SetStatsAndExp()
    {
        _hpText.text = $"{_battler.Hp} / {_battler.MaxHp}";
        _strengthText.text = _battler.Strength.ToString();
        _enduranceText.text = _battler.Endurance.ToString();
        _intelligenceText.text = _battler.Intelligence.ToString();
        _fortitudeText.text = _battler.Fortitude.ToString();
        _agilityText.text = _battler.Agility.ToString();

        _expText.text = _battler.Exp.ToString();

        if (_battler.Level == GlobalSettings.Instance.MaxLevel)
        {
            _expToNextLevelText.text = "Max Level";
        }
        else
        {
            int expToNextLevel = _battler.Base.GetExpForLevel(_battler.Level + 1) - _battler.Exp;
            _expToNextLevelText.text = expToNextLevel.ToString();
        }

        _expBar.transform.localScale = new Vector3(_battler.GetNormalizedExp(), 1f, 1f);
    }

    public void SetMoves()
    {
        for (int i = 0; i < _moveNames.Count; i++)
        {
            if (i < _battler.Moves.Count)
            {
                Move move = _battler.Moves[i];
                _moveTypes[i].text = move.Base.Type.ToString();
                _moveNames[i].text = move.Base.Name;
                _moveSP[i].text = $"SP {move.Sp} / {move.Base.SP}";
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

        if (_battler.Moves.Count == 0 || _selectedItem < 0 || _selectedItem >= _battler.Moves.Count)
        {
            _moveDescriptionText.text = string.Empty;
            _movePowerText.text = "-";
            _moveAccuracyText.text = "-";
            return;
        }

        UpdateMoveDetails(_battler.Moves[_selectedItem]);
    }

    private void UpdateMoveDetails(Move move)
    {
        _moveDescriptionText.text = move.Base.Description;
        _movePowerText.text = move.Base.Power > 0 ? move.Base.Power.ToString() : "-";
        _moveAccuracyText.text = move.Base.Accuracy > 0 ? move.Base.Accuracy.ToString() : "-";
    }
}