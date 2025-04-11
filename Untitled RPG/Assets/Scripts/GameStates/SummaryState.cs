using System.Collections.Generic;
using UnityEngine;
using Util.StateMachine;
using Util.GenericSelectionUI;
using System.Collections;

public class SummaryState : State<GameController>
{
    [SerializeField] private SummaryScreenUI _summaryScreenUI;

    private int _selectedPage = 0;
    private List<Battler> _currentBattlerList;
    private GameController _gameController;
    private DummySelectionUI _battlerSelectionUI;
    private DummySelectionUI _pageSelectionUI;
    private bool _isProcessingBack = false;

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

        // Use the provided battler list if available; otherwise, get from the player's BattleParty.
        _currentBattlerList = BattlersList ?? PlayerController.Instance.GetComponent<BattleParty>().Battlers;
        if (_currentBattlerList == null || _currentBattlerList.Count == 0)
        {
            _currentBattlerList = new List<Battler>();
        }

        if (SelectedBattlerIndex < 0 || SelectedBattlerIndex >= _currentBattlerList.Count)
        {
            SelectedBattlerIndex = 0;
        }

        _ = StartCoroutine(EnterState());

        // Initialize the selection UIs.
        InitializeBattlerSelectionUI();
        InitializePageSelectionUI();
    }

    private IEnumerator EnterState()
    {
        _gameController.StateMachine.Push(CutsceneState.Instance);
        yield return Fader.Instance.FadeIn(0.5f);

        // Activate the summary UI and show initial details.
        _summaryScreenUI.gameObject.SetActive(true);
        _summaryScreenUI.EnableInput(true);
        _isProcessingBack = false;
        if (_currentBattlerList.Count > 0)
        {
            _summaryScreenUI.SetBasicDetails(_currentBattlerList[SelectedBattlerIndex]);
        }
        _summaryScreenUI.ShowPage(_selectedPage);

        yield return Fader.Instance.FadeOut(0.5f);
        _gameController.StateMachine.Pop();
    }

    public override void Execute()
    {
        if (_isProcessingBack)
        {
            return;
        }

        // Only update the selection UIs if the summary UI isn't in move selection mode.
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
            // If on Move Details page and not already in move selection, enter move selection.
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
                _ = StartCoroutine(LeaveState());
                return;
            }
        }

        // Update the summary UI itself.
        _summaryScreenUI.HandleUpdate();
    }

    public override void Exit()
    {
        _summaryScreenUI.gameObject.SetActive(false);
        _selectedPage = 0;
        BattlersList = null;

        if (_battlerSelectionUI != null)
        {
            _battlerSelectionUI.OnIndexChanged -= OnBattlerIndexChanged;
            Destroy(_battlerSelectionUI);
            _battlerSelectionUI = null;
        }
        if (_pageSelectionUI != null)
        {
            _pageSelectionUI.OnIndexChanged -= OnPageIndexChanged;
            Destroy(_pageSelectionUI);
            _pageSelectionUI = null;
        }
    }

    private IEnumerator LeaveState()
    {
        _isProcessingBack = true;
        _summaryScreenUI.EnableInput(false);
        _battlerSelectionUI.EnableInput(false);
        _pageSelectionUI.EnableInput(false);
        yield return Fader.Instance.FadeIn(0.5f);

        _gameController.StateMachine.ChangeState(CutsceneState.Instance);
        yield return Fader.Instance.FadeOut(0.5f);

        _gameController.StateMachine.Pop();
    }

    private void InitializeBattlerSelectionUI()
    {
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
        _battlerSelectionUI.EnableInput(true);
        _battlerSelectionUI.OnIndexChanged += OnBattlerIndexChanged;
    }

    private void OnBattlerIndexChanged(int index)
    {
        SelectedBattlerIndex = index;
        if (_currentBattlerList.Count > 0)
        {
            _summaryScreenUI.SetBasicDetails(_currentBattlerList[SelectedBattlerIndex]);
            _summaryScreenUI.ShowPage(_selectedPage);
        }
    }

    private void InitializePageSelectionUI()
    {
        _pageSelectionUI = _gameController.gameObject.AddComponent<DummySelectionUI>();
        _pageSelectionUI.SetSelectionSettings(SelectionType.Grid, 2);
        _pageSelectionUI.IgnoreVerticalInput = true;

        List<DummySelectable> pageItems = new() { new DummySelectable(), new DummySelectable() };
        _pageSelectionUI.SetItems(pageItems);
        _pageSelectionUI.SetSelectedIndex(_selectedPage);
        _pageSelectionUI.EnableInput(true);
        _pageSelectionUI.OnIndexChanged += OnPageIndexChanged;
    }

    private void OnPageIndexChanged(int index)
    {
        _selectedPage = index;
        _summaryScreenUI.ShowPage(_selectedPage);
    }
}