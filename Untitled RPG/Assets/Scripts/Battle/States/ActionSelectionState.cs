using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.StateMachine;

public class ActionSelectionState : State<BattleSystem>
{
    [field: SerializeField] public ActionSelectionUI SelectionUI { get; private set; }

    private BattleSystem _battleSystem;
    private BattleUnit _activeUnit;
    private int _prevSelectionIndex = 0;
    private TextMeshProUGUI _talkText;

    public static ActionSelectionState Instance { get; private set; }

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

        // Initialize the talk text component from the UI.
        InitializeTalkText();
    }

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _activeUnit = _battleSystem.SelectingUnit;

        if (SelectionUI == null)
        {
            Debug.LogError("ActionSelectionUI is not assigned.");
            return;
        }

        // Set Talk text color based on whether the unit is a commander.
        _talkText.color = !_activeUnit.Battler.IsCommander ? GlobalSettings.Instance.EmptyColor : Color.white;

        // Restore saved selection if it exists; otherwise, default to 0.
        if (_battleSystem.UnitSelectionIndices.TryGetValue(_activeUnit, out int savedSelection))
        {
            SelectionUI.SetSelectedIndex(savedSelection);
        }
        else
        {
            SelectionUI.ResetSelection();
        }

        SelectionUI.gameObject.SetActive(true);
        SelectionUI.OnSelected += OnActionSelected;
        SelectionUI.OnBack += OnBack;

        // Display dialogue and set the selecting unit as active.
        _battleSystem.DialogueBox.SetDialogue($"Choose an action for {_activeUnit.Battler.Base.Name}!");
        _activeUnit.SetSelected(true);
    }

    public override void Execute()
    {
        SelectionUI.HandleUpdate();

        if (!_activeUnit.Battler.IsCommander)
        {
            // If the unit is not the commander, ignore the talk option.
            if (SelectionUI.SelectedIndex == 1)
            {
                int newIndex = _prevSelectionIndex == 0 ? 2
                    : _prevSelectionIndex == 2 ? 0
                    : 4;
                SelectionUI.SetSelectedIndex(newIndex);
            }
            _talkText.color = GlobalSettings.Instance.EmptyColor;
        }

        _prevSelectionIndex = SelectionUI.SelectedIndex;
    }

    public override void Exit()
    {
        // Save the current selection index for the active unit.
        _battleSystem.UnitSelectionIndices[_activeUnit] = SelectionUI.SelectedIndex;

        SelectionUI.gameObject.SetActive(false);
        SelectionUI.OnSelected -= OnActionSelected;
        SelectionUI.OnBack -= OnBack;
    }

    private void InitializeTalkText()
    {
        List<TextSlot> textSlots = SelectionUI.GetComponentsInChildren<TextSlot>().ToList();
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

    private void HandleMoveSelection()
    {
        MoveSelectionState.Instance.Moves = _activeUnit.Battler.Moves;
        _battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
    }

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
                    TargetUnits = selectedItem.Target is MoveTarget.Self ? new List<BattleUnit> { _activeUnit }
                        : selectedItem.Target is MoveTarget.AllAllies ? _battleSystem.PlayerUnits
                        : selectedItem.Target is MoveTarget.AllEnemies ? _battleSystem.EnemyUnits
                        : _battleSystem.PlayerUnits.Where(u => u != _activeUnit).Concat(_battleSystem.EnemyUnits).ToList()
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

    private void HandleGuardAction()
    {
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Guard
        });
    }

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

    private void HandleRunAction()
    {
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Run
        });
    }

    private void OnBack()
    {
        _battleSystem.UndoBattleAction();
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
    }
}