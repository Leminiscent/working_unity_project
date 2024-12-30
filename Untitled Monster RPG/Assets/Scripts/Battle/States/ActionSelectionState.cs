using System.Collections;
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
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.Talk,
                    TargetUnit = _battleSystem.EnemyUnits[0] // TODO: Implement multiple enemies

                });
                break;
            case 2:
                // Item
                StartCoroutine(GoToInventoryState());
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

    private IEnumerator GoToInventoryState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;
        if (selectedItem != null)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.UseItem,
                SelectedItem = selectedItem
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
