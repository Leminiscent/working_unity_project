using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    GameController gameController;
    MonsterParty playerParty;
    bool isSwitchingPosition;
    int selectedSwitchToIndex = 0;

    public Monster SelectedMonster { get; private set; }

    public static PartyState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerParty = PlayerController.Instance.GetComponent<MonsterParty>();
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
        StartCoroutine(MonsterSelectedAction(selection));
    }

    IEnumerator MonsterSelectedAction(int selectedMonsterIndex)
    {
        State<GameController> prevState = gameController.StateMachine.GetPrevState();

        if (prevState == InventoryState.Instance)
        {
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.Instance)
        {
            BattleState battleState = prevState as BattleState;

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
                SummaryState.Instance.SelectedMonsterIndex = selectedMonsterIndex;
                yield return gameController.StateMachine.PushAndWait(SummaryState.Instance);
            }
            else
            {
                yield break;
            }
        }
        else
        {
            if (isSwitchingPosition)
            {
                if (selectedSwitchToIndex == selectedMonsterIndex)
                {
                    partyScreen.SetMessageText("You can't switch with the same monster!");
                    yield break;
                }

                isSwitchingPosition = false;
                (playerParty.Monsters[selectedMonsterIndex], playerParty.Monsters[selectedSwitchToIndex]) = (playerParty.Monsters[selectedSwitchToIndex], playerParty.Monsters[selectedMonsterIndex]);
                playerParty.PartyUpdated();
                yield break;
            }

            DynamicMenuState.Instance.MenuItems = new List<string>
            {
                "Summary",
                "Switch",
                "Back"
            };
            yield return gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);

            if (DynamicMenuState.Instance.SelectedItem == 0)
            {
                SummaryState.Instance.SelectedMonsterIndex = selectedMonsterIndex;
                yield return gameController.StateMachine.PushAndWait(SummaryState.Instance);
            }
            else if (DynamicMenuState.Instance.SelectedItem == 1)
            {
                isSwitchingPosition = true;
                selectedSwitchToIndex = selectedMonsterIndex;
                partyScreen.SetMessageText($"Choose a monster to switch with {playerParty.Monsters[selectedMonsterIndex].Base.Name}.");
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

        State<GameController> prevState = gameController.StateMachine.GetPrevState();

        if (prevState == BattleState.Instance)
        {
            BattleState battleState = prevState as BattleState;

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
