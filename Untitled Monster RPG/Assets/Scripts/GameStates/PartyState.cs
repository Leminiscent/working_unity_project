using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class PartyState : State<GameController>
{
    [SerializeField] private PartyScreen _partyScreen;

    private GameController _gameController;
    private MonsterParty _playerParty;
    private bool _isSwitchingPosition;
    private int _selectedSwitchToIndex = 0;

    public Monster SelectedMonster { get; private set; }
    public static PartyState Instance { get; private set; }

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

    private void Start()
    {
        _playerParty = PlayerController.Instance.GetComponent<MonsterParty>();
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        SelectedMonster = null;
        _partyScreen.gameObject.SetActive(true);
        _partyScreen.OnSelected += OnMonsterSelected;
        _partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        _partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        _partyScreen.gameObject.SetActive(false);
        _partyScreen.OnSelected -= OnMonsterSelected;
        _partyScreen.OnBack -= OnBack;
    }

    private void OnMonsterSelected(int selection)
    {
        SelectedMonster = _partyScreen.SelectedMember;
        StartCoroutine(MonsterSelectedAction(selection));
    }

    private IEnumerator MonsterSelectedAction(int selectedMonsterIndex)
    {
        State<GameController> prevState = _gameController.StateMachine.GetPrevState();

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
            yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);
            if (DynamicMenuState.Instance.SelectedItem == 0)
            {
                if (SelectedMonster.Hp <= 0)
                {
                    _partyScreen.SetMessageText($"{SelectedMonster.Base.Name} is unable to fight!");
                    yield break;
                }
                if (battleState.BattleSystem.PlayerUnits.Any(u => u.Monster == SelectedMonster))
                {
                    _partyScreen.SetMessageText($"{SelectedMonster.Base.Name} is already in battle!");
                    yield break;
                }
                if (battleState.BattleSystem.UnableToSwitch(SelectedMonster))
                {
                    _partyScreen.SetMessageText($"{SelectedMonster.Base.Name} is already preparing for battle!");
                    yield break;
                }

                _gameController.StateMachine.Pop();
            }
            else if (DynamicMenuState.Instance.SelectedItem == 1)
            {
                SummaryState.Instance.SelectedMonsterIndex = selectedMonsterIndex;
                yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);
            }
            else
            {
                yield break;
            }
        }
        else
        {
            if (_isSwitchingPosition)
            {
                if (_selectedSwitchToIndex == selectedMonsterIndex)
                {
                    _partyScreen.SetMessageText("You can't switch with the same monster!");
                    yield break;
                }

                _isSwitchingPosition = false;
                (_playerParty.Monsters[selectedMonsterIndex], _playerParty.Monsters[_selectedSwitchToIndex]) = (_playerParty.Monsters[_selectedSwitchToIndex], _playerParty.Monsters[selectedMonsterIndex]);
                _playerParty.PartyUpdated();
                yield break;
            }

            DynamicMenuState.Instance.MenuItems = new List<string>
            {
                "Summary",
                "Switch",
                "Back"
            };
            yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);

            if (DynamicMenuState.Instance.SelectedItem == 0)
            {
                SummaryState.Instance.SelectedMonsterIndex = selectedMonsterIndex;
                yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);
            }
            else if (DynamicMenuState.Instance.SelectedItem == 1)
            {
                _isSwitchingPosition = true;
                _selectedSwitchToIndex = selectedMonsterIndex;
                _partyScreen.SetMessageText($"Choose a monster to switch with {_playerParty.Monsters[selectedMonsterIndex].Base.Name}.");
            }
            else
            {
                yield break;
            }
        }
    }

    private IEnumerator GoToUseItemState()
    {
        yield return _gameController.StateMachine.PushAndWait(UseItemState.Instance);
        _gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        SelectedMonster = null;

        State<GameController> prevState = _gameController.StateMachine.GetPrevState();

        if (prevState == BattleState.Instance)
        {
            BattleState battleState = prevState as BattleState;

            if (battleState.BattleSystem.PlayerUnits.Any(static u => u.Monster.Hp <= 0))
            {
                _partyScreen.SetMessageText("You have to choose a monster!");
                return;
            }
            _partyScreen.gameObject.SetActive(false);
        }
        _gameController.StateMachine.Pop();
    }
}
