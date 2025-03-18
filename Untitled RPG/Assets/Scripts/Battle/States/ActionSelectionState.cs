using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.StateMachine;

/// <summary>
/// Represents the state where a player selects an action during battle.
/// </summary>
public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] private ActionSelectionUI _selectionUI;

    private BattleSystem _battleSystem;
    private int _prevSelectionIndex = 0;
    private TextMeshProUGUI _talkText;

    public static ActionSelectionState Instance { get; private set; }
    public ActionSelectionUI SelectionUI => _selectionUI;

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

    /// <summary>
    /// Enters the Action Selection state. Initializes UI, sets up event listeners, and displays dialogue.
    /// </summary>
    /// <param name="owner">The BattleSystem that owns this state.</param>
    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;

        if (_selectionUI == null)
        {
            Debug.LogError("ActionSelectionUI is not assigned.");
            return;
        }

        _selectionUI.gameObject.SetActive(true);
        _selectionUI.OnSelected += OnActionSelected;
        _selectionUI.OnBack += OnBack;

        // Display dialogue and set the selecting unit as active.
        _battleSystem.DialogueBox.SetDialogue($"Choose an action for {_battleSystem.SelectingUnit.Battler.Base.Name}!");
        _battleSystem.SelectingUnit.SetSelected(true);

        // Initialize the talk text component from the UI.
        InitializeTalkText();

        // Set text color based on whether the unit is a commander.
        _talkText.color = !_battleSystem.SelectingUnit.Battler.IsCommander ? GlobalSettings.Instance.EmptyColor : Color.white;
    }

    /// <summary>
    /// Updates the state by handling input and updating the selection UI.
    /// </summary>
    public override void Execute()
    {
        _selectionUI.HandleUpdate();

        if (!_battleSystem.SelectingUnit.Battler.IsCommander)
        {
            // If the unit is not the commander, ignore the talk option.
            if (_selectionUI.SelectedIndex == 1)
            {
                int newIndex = _prevSelectionIndex == 0 ? 2
                    : _prevSelectionIndex == 2 ? 0
                    : 4;
                _selectionUI.SetSelectedIndex(newIndex);
            }
            _talkText.color = GlobalSettings.Instance.EmptyColor;
        }

        _prevSelectionIndex = _selectionUI.SelectedIndex;
    }

    /// <summary>
    /// Exits the Action Selection state and unsubscribes from UI events.
    /// </summary>
    public override void Exit()
    {
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnActionSelected;
        _selectionUI.OnBack -= OnBack;
    }

    /// <summary>
    /// Safely initializes the talk text component from the ActionSelectionUI.
    /// </summary>
    private void InitializeTalkText()
    {
        List<TextSlot> textSlots = _selectionUI.GetComponentsInChildren<TextSlot>().ToList();
        if (textSlots.Count > 1)
        {
            _talkText = textSlots[1].GetComponent<TextMeshProUGUI>();
            if (_talkText == null)
            {
                Debug.LogWarning("Expected TextMeshProUGUI component not found in the second TextSlot.");
            }
        }
        else
        {
            Debug.LogWarning("Expected at least two TextSlot components in the ActionSelectionUI.");
        }
    }

    /// <summary>
    /// Called when an action is selected in the UI. Dispatches to the appropriate handler.
    /// </summary>
    /// <param name="selection">The index of the selected action.</param>
    private void OnActionSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                HandleMoveSelection();
                break;
            case 1:
                _ = StartCoroutine(HandleRecruitAction());
                break;
            case 2:
                _ = StartCoroutine(HandleItemSelection());
                break;
            case 3:
                HandleGuardAction();
                break;
            case 4:
                _ = StartCoroutine(HandlePartySwitch());
                break;
            case 5:
                HandleRunAction();
                break;
            default:
                Debug.LogWarning($"Unhandled action selection: {selection}");
                break;
        }
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    /// <summary>
    /// Handles the move selection action.
    /// </summary>
    private void HandleMoveSelection()
    {
        MoveSelectionState.Instance.Moves = _battleSystem.SelectingUnit.Battler.Moves;
        _battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
    }

    /// <summary>
    /// Coroutine that handles selecting a recruit target.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator HandleRecruitAction()
    {
        int recruitTarget = 0;
        if (_battleSystem.EnemyUnits.Count > 1)
        {
            TargetSelectionState.Instance.IsTargetingAllies = false;
            yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
            if (!TargetSelectionState.Instance.SelectionMade)
            {
                yield break;
            }
            recruitTarget = TargetSelectionState.Instance.SelectedTarget;
        }
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Talk,
            TargetUnits = new List<BattleUnit> { _battleSystem.EnemyUnits[recruitTarget] }
        });
    }

    /// <summary>
    /// Coroutine that handles item selection and target assignment.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator HandleItemSelection()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;
        if (selectedItem != null)
        {
            // If the item's target type doesn't require specific selection, determine targets automatically.
            if (selectedItem.Target is MoveTarget.Self or MoveTarget.AllAllies or MoveTarget.AllEnemies or MoveTarget.Others)
            {
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.UseItem,
                    SelectedItem = selectedItem,
                    TargetUnits = selectedItem.Target is MoveTarget.Self ? new List<BattleUnit> { _battleSystem.SelectingUnit }
                        : selectedItem.Target is MoveTarget.AllAllies ? _battleSystem.PlayerUnits
                        : selectedItem.Target is MoveTarget.AllEnemies ? _battleSystem.EnemyUnits
                        : _battleSystem.PlayerUnits.Where(u => u != _battleSystem.SelectingUnit).Concat(_battleSystem.EnemyUnits).ToList()
                });
                yield break;
            }

            int itemTarget = 0;
            if ((selectedItem.Target is MoveTarget.Enemy && _battleSystem.EnemyUnits.Count > 1) ||
                (selectedItem.Target is MoveTarget.Ally && _battleSystem.PlayerUnits.Count > 1))
            {
                TargetSelectionState.Instance.IsTargetingAllies = selectedItem.Target is MoveTarget.Ally;
                yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
                if (!TargetSelectionState.Instance.SelectionMade)
                {
                    yield break;
                }
                itemTarget = TargetSelectionState.Instance.SelectedTarget;
            }

            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.UseItem,
                SelectedItem = selectedItem,
                TargetUnits = selectedItem.Target is MoveTarget.Enemy ? new List<BattleUnit> { _battleSystem.EnemyUnits[itemTarget] }
                    : new List<BattleUnit> { _battleSystem.PlayerUnits[itemTarget] }
            });
        }
    }

    /// <summary>
    /// Handles the guard action.
    /// </summary>
    private void HandleGuardAction()
    {
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Guard
        });
    }

    /// <summary>
    /// Coroutine that handles switching to the party state.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator HandlePartySwitch()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);

        Battler selectedBattler = PartyState.Instance.SelectedMember;
        if (selectedBattler != null)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.SwitchBattler,
                SelectedBattler = selectedBattler
            });
        }
    }

    /// <summary>
    /// Handles the run action.
    /// </summary>
    private void HandleRunAction()
    {
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Run
        });
    }

    /// <summary>
    /// Called when the back action is triggered from the UI.
    /// </summary>
    private void OnBack()
    {
        _battleSystem.UndoBattleAction();
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
    }
}