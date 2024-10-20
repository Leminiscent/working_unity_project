using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;
using Utils.StateMachine;

public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] MoveSelectionUI selectionUI;
    [SerializeField] GameObject moveDetailsUI;
    BattleSystem battleSystem;

    public List<Move> Moves { get; set; }

    public static MoveSelectionState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(BattleSystem owner)
    {
        battleSystem = owner;
        selectionUI.SetMoves(Moves);

        if (Moves.Where(m => m.SP > 0).Count() == 0)
        {
            battleSystem.SelectedMove = -1;
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
            return;
        }
        
        selectionUI.gameObject.SetActive(true);
        moveDetailsUI.SetActive(true);
        battleSystem.DialogueBox.EnableDialogueText(false);
        selectionUI.OnSelected += OnMoveSelected;
        selectionUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        moveDetailsUI.SetActive(false);
        battleSystem.DialogueBox.EnableDialogueText(true);
        selectionUI.OnSelected -= OnMoveSelected;
        selectionUI.OnBack -= OnBack;
        selectionUI.ClearItems();
    }

    void OnMoveSelected(int selection)
    {
        battleSystem.SelectedMove = selection;
        battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
    }

    void OnBack()
    {
        battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
    }
}
