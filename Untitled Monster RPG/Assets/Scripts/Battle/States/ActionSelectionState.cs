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
        _battleSystem.DialogueBox.SetDialogue("Choose an action!");
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
                _battleSystem.SelectedAction = BattleAction.Fight;
                MoveSelectionState.Instance.Moves = _battleSystem.PlayerUnits.Monster.Moves;
                _battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
                break;
            case 1:
                // Talk
                _battleSystem.SelectedAction = BattleAction.Talk;
                _battleSystem.StateMachine.ChangeState(RecruitmentState.Instance);
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
                _battleSystem.SelectedAction = BattleAction.Run;
                _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
                break;
            default:
                Debug.LogWarning("Invalid action selection: " + selection);
                break;
        }
    }

    private IEnumerator GoToInventoryState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;
        if (selectedItem != null)
        {
            _battleSystem.SelectedAction = BattleAction.UseItem;
            _battleSystem.SelectedItem = selectedItem;
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    private IEnumerator GoToPartyState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);

        Monster selectedMonster = PartyState.Instance.SelectedMonster;
        if (selectedMonster != null)
        {
            _battleSystem.SelectedAction = BattleAction.SwitchMonster;
            _battleSystem.SelectedMonster = selectedMonster;
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }
}
