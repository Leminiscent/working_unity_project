using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;
using Utils.GenericSelectionUI;

public class SummaryState : State<GameController>
{
    [SerializeField] private SummaryScreenUI _summaryScreenUI;

    private int _selectedPage = 0;
    private List<Battler> _currentBattlerList;
    private GameController _gameController;
    private DummySelectionUI _battlerSelectionUI;
    private DummySelectionUI _pageSelectionUI;

    public int SelectedBattlerIndex { get; set; }
    public List<Battler> BattlersList { get; set; }
    public static SummaryState Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _currentBattlerList = BattlersList ?? PlayerController.Instance.GetComponent<BattleParty>().Battlers;
        if (_currentBattlerList == null || _currentBattlerList.Count == 0)
        {
            _currentBattlerList = new List<Battler>();
        }
        if (SelectedBattlerIndex < 0 || SelectedBattlerIndex >= _currentBattlerList.Count)
        {
            SelectedBattlerIndex = 0;
        }
        _summaryScreenUI.gameObject.SetActive(true);
        if (_currentBattlerList.Count > 0)
        {
            _summaryScreenUI.SetBasicDetails(_currentBattlerList[SelectedBattlerIndex]);
        }
        _summaryScreenUI.ShowPage(_selectedPage);

        // Initialize battler selection UI.
        _battlerSelectionUI = _gameController.gameObject.AddComponent<DummySelectionUI>();
        _battlerSelectionUI.SetSelectionSettings(SelectionType.List, 1);
        _battlerSelectionUI.IgnoreHorizontalInput = true;
        List<DummySelectable> battlerItems = new();
        for (int i = 0; i < _currentBattlerList.Count; i++)
        {
            battlerItems.Add(new DummySelectable());
        }
        _battlerSelectionUI.SetItems(battlerItems);
        _battlerSelectionUI.SetSelectedIndex(SelectedBattlerIndex);
        _battlerSelectionUI.OnIndexChanged += (index) =>
        {
            SelectedBattlerIndex = index;
            if (_currentBattlerList.Count > 0)
            {
                _summaryScreenUI.SetBasicDetails(_currentBattlerList[SelectedBattlerIndex]);
                _summaryScreenUI.ShowPage(_selectedPage);
            }
        };

        // Initialize page selection UI.
        _pageSelectionUI = _gameController.gameObject.AddComponent<DummySelectionUI>();
        _pageSelectionUI.SetSelectionSettings(SelectionType.Grid, 2);
        _pageSelectionUI.IgnoreVerticalInput = true;
        List<DummySelectable> pageItems = new() { new(), new() };
        _pageSelectionUI.SetItems(pageItems);
        _pageSelectionUI.SetSelectedIndex(_selectedPage);
        _pageSelectionUI.OnIndexChanged += (index) =>
        {
            _selectedPage = index;
            _summaryScreenUI.ShowPage(_selectedPage);
        };
    }

    public override void Execute()
    {
        if (!_summaryScreenUI.InMoveSelection)
        {
            if (_battlerSelectionUI != null)
            {
                _battlerSelectionUI.HandleUpdate();
            }
            if (_pageSelectionUI != null)
            {
                _pageSelectionUI.HandleUpdate();
            }
        }

        if (Input.GetButtonDown("Action"))
        {
            if (_selectedPage == 1 && !_summaryScreenUI.InMoveSelection)
            {
                _summaryScreenUI.InMoveSelection = true;
                AudioManager.Instance.PlaySFX(AudioID.UISelect);
            }
        }
        else if (Input.GetButtonDown("Back"))
        {
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            if (_summaryScreenUI.InMoveSelection)
            {
                _summaryScreenUI.ResetSelection();
                _summaryScreenUI.InMoveSelection = false;
            }
            else
            {
                _gameController.StateMachine.Pop();
                return;
            }
        }

        _summaryScreenUI.HandleUpdate();
    }

    public override void Exit()
    {
        _summaryScreenUI.gameObject.SetActive(false);
        _selectedPage = 0;
        BattlersList = null;

        if (_battlerSelectionUI != null)
        {
            Destroy(_battlerSelectionUI);
            _battlerSelectionUI = null;
        }
        if (_pageSelectionUI != null)
        {
            Destroy(_pageSelectionUI);
            _pageSelectionUI = null;
        }
    }
}
