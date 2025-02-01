using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] private ActionSelectionUI _selectionUI;

    private BattleSystem _battleSystem;

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

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _selectionUI.gameObject.SetActive(true);
        _selectionUI.OnSelected += OnActionSelected;
        _battleSystem.DialogueBox.SetDialogue($"Choose an action for {_battleSystem.SelectingUnit.Monster.Base.Name}!");
    }

    public override void Execute()
    {
        _selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnActionSelected;
    }

    private void OnActionSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                // Fight
                MoveSelectionState.Instance.Moves = _battleSystem.SelectingUnit.Monster.Moves;
                _battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
                break;
            case 1:
                // Talk
                StartCoroutine(SelectRecruitTarget());
                break;
            case 2:
                // Item
                StartCoroutine(SelectItemAndTarget());
                break;
            case 3:
                // Guard
                // TODO: Implement guard action
                break;
            case 4:
                // Switch
                StartCoroutine(GoToPartyState());
                break;
            case 5:
                // Run
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.Run
                });
                break;
            default:
                break;
        }
    }

    private IEnumerator SelectRecruitTarget()
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

    private IEnumerator SelectItemAndTarget()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;
        if (selectedItem != null)
        {
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
            if ((selectedItem.Target is MoveTarget.Enemy && _battleSystem.EnemyUnits.Count > 1) || (selectedItem.Target is MoveTarget.Ally && _battleSystem.PlayerUnits.Count > 1))
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

    private IEnumerator GoToPartyState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);

        Monster selectedMonster = PartyState.Instance.SelectedMonster;
        if (selectedMonster != null)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.SwitchMonster,
                SelectedMonster = selectedMonster
            });
        }
    }
}