using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using Utils.StateMachine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;
    BattleSystem battleSystem;

    public static ActionSelectionState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(BattleSystem owner)
    {
        battleSystem = owner;
        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnActionSelected;
        battleSystem.DialogueBox.SetDialogue("Choose an action!");
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnActionSelected;
    }

    void OnActionSelected(int selection)
    {
        if (selection == 0)
        {
            // Fight
            battleSystem.SelectedAction = BattleAction.Fight;
            MoveSelectionState.Instance.Moves = battleSystem.PlayerUnit.Monster.Moves;
            battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
        }
        else if (selection == 1)
        {
            // Talk
        }
        else if (selection == 2)
        {
            // Item
        }
        else if (selection == 3)
        {
            // Guard
        }
        else if (selection == 4)
        {
            // Switch
            StartCoroutine(GoToPartyState());
        }
        else if (selection == 5)
        {
            // Run
            battleSystem.SelectedAction = BattleAction.Run;
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    IEnumerator GoToPartyState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);

        var selectedMonster = PartyState.Instance.SelectedMonster;

        if (selectedMonster != null)
        {
            battleSystem.SelectedAction = BattleAction.SwitchMonster;
            battleSystem.SelectedMonster = selectedMonster;
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }
}
