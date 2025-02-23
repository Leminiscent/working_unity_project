using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;
using Utils.GenericSelectionUI;

public class SummaryState : State<GameController>
{
    [SerializeField] private SummaryScreenUI _summaryScreenUI;

    private int _selectedPage = 0;
    private List<Monster> _currentMonsterList;
    private GameController _gameController;
    private DummySelectionUI _monsterSelectionUI;
    private DummySelectionUI _pageSelectionUI;

    public int SelectedMonsterIndex { get; set; }
    public List<Monster> MonstersList { get; set; }
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
        _currentMonsterList = MonstersList ?? PlayerController.Instance.GetComponent<MonsterParty>().Monsters;
        if (_currentMonsterList == null || _currentMonsterList.Count == 0)
        {
            _currentMonsterList = new List<Monster>();
        }
        if (SelectedMonsterIndex < 0 || SelectedMonsterIndex >= _currentMonsterList.Count)
        {
            SelectedMonsterIndex = 0;
        }
        _summaryScreenUI.gameObject.SetActive(true);
        if (_currentMonsterList.Count > 0)
        {
            _summaryScreenUI.SetBasicDetails(_currentMonsterList[SelectedMonsterIndex]);
        }
        _summaryScreenUI.ShowPage(_selectedPage);

        // Initialize monster selection UI.
        _monsterSelectionUI = _gameController.gameObject.AddComponent<DummySelectionUI>();
        _monsterSelectionUI.SetSelectionSettings(SelectionType.List, 1);
        _monsterSelectionUI.IgnoreHorizontalInput = true;
        List<DummySelectable> monsterItems = new();
        for (int i = 0; i < _currentMonsterList.Count; i++)
        {
            monsterItems.Add(new DummySelectable());
        }
        _monsterSelectionUI.SetItems(monsterItems);
        _monsterSelectionUI.SetSelectedIndex(SelectedMonsterIndex);
        _monsterSelectionUI.OnIndexChanged += (index) =>
        {
            SelectedMonsterIndex = index;
            if (_currentMonsterList.Count > 0)
            {
                _summaryScreenUI.SetBasicDetails(_currentMonsterList[SelectedMonsterIndex]);
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
            if (_monsterSelectionUI != null)
            {
                _monsterSelectionUI.HandleUpdate();
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
        MonstersList = null;

        if (_monsterSelectionUI != null)
        {
            Destroy(_monsterSelectionUI);
            _monsterSelectionUI = null;
        }
        if (_pageSelectionUI != null)
        {
            Destroy(_pageSelectionUI);
            _pageSelectionUI = null;
        }
    }
}
