using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class GamePartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    GameController gameController;

    public static GamePartyState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        partyScreen.gameObject.SetActive(true);
        partyScreen.OnSelected += OnMonsterSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnMonsterSelected;
        partyScreen.OnBack -= OnBack;
    }

    void OnMonsterSelected(int selection)
    {
        if (gameController.StateMachine.GetPrevState() == InventoryState.Instance)
        {
            StartCoroutine(GoToUseItemState());
        }
        else
        {
            // TODO: Open Monster Summary Screen
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gameController.StateMachine.PushAndWait(UseItemState.Instance);
        gameController.StateMachine.Pop();
    }

    void OnBack()
    {
        gameController.StateMachine.Pop();
    }
}
