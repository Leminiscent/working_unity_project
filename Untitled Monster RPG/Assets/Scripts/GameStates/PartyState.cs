using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    GameController gameController;

    public Monster SelectedMonster { get; private set; }

    public static PartyState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        SelectedMonster = null;
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
        SelectedMonster = partyScreen.SelectedMember;
        StartCoroutine(MonsterSelectedAction());
    }

    IEnumerator MonsterSelectedAction()
    {
        var prevState = gameController.StateMachine.GetPrevState();

        if (prevState == InventoryState.Instance)
        {
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.Instance)
        {
            var battleState = prevState as BattleState;

            DynamicMenuState.Instance.MenuItems = new List<string>
            {
                "Switch",
                "Summary",
                "Back"
            };
            yield return gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);
            if (DynamicMenuState.Instance.SelectedItem == 0)
            {
                if (SelectedMonster.HP <= 0)
                {
                    partyScreen.SetMessageText(SelectedMonster.Base.Name + " is unable to fight!");
                    yield break;
                }
                if (SelectedMonster == battleState.BattleSystem.PlayerUnit.Monster)
                {
                    partyScreen.SetMessageText(SelectedMonster.Base.Name + " is already in battle!");
                    yield break;
                }
                
                gameController.StateMachine.Pop();
            }
            else if (DynamicMenuState.Instance.SelectedItem == 1)
            {
                // Summary
            }
            else
            {
                yield break;
            }
        }
        else
        {
            DynamicMenuState.Instance.MenuItems = new List<string>
            {
                "Summary",
                "Switch",
                "Back"
            };
            yield return gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);
            if (DynamicMenuState.Instance.SelectedItem == 0)
            {
                // Summary
            }
            else if (DynamicMenuState.Instance.SelectedItem == 1)
            {
                // Switch
            }
            else
            {
                yield break;
            }
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gameController.StateMachine.PushAndWait(UseItemState.Instance);
        gameController.StateMachine.Pop();
    }

    void OnBack()
    {
        SelectedMonster = null;

        var prevState = gameController.StateMachine.GetPrevState();

        if (prevState == BattleState.Instance)
        {
            var battleState = prevState as BattleState;

            if (battleState.BattleSystem.PlayerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
        }
        gameController.StateMachine.Pop();
    }
}
