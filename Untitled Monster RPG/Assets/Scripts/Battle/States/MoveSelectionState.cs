using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] private MoveSelectionUI _selectionUI;
    [SerializeField] private GameObject _moveDetailsUI;

    private BattleSystem _battleSystem;

    public List<Move> Moves { get; set; }
    public static MoveSelectionState Instance { get; private set; }

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
        _selectionUI.SetMoves(Moves);

        if (Moves.Where(static m => m.Sp > 0).Count() == 0)
        {
            _battleSystem.SelectedMove = -1;
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
            return;
        }

        _selectionUI.gameObject.SetActive(true);
        _moveDetailsUI.SetActive(true);
        _battleSystem.DialogueBox.EnableDialogueText(false);
        _selectionUI.OnSelected += OnMoveSelected;
        _selectionUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        _selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        _selectionUI.gameObject.SetActive(false);
        _moveDetailsUI.SetActive(false);
        _battleSystem.DialogueBox.EnableDialogueText(true);
        _selectionUI.OnSelected -= OnMoveSelected;
        _selectionUI.OnBack -= OnBack;
        _selectionUI.ClearItems();
    }

    private void OnMoveSelected(int selection)
    {
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Fight,
            SelectedMove = Moves[selection]
        });
    }

    private void OnBack()
    {
        _battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
    }
}
