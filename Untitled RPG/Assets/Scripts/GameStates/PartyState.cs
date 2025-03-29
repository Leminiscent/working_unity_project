using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class PartyState : State<GameController>
{
    [SerializeField] private PartyScreen _partyScreen;

    private GameController _gameController;
    private State<GameController> _prevState;
    private BattleParty _playerParty;
    private bool _isSwitchingPosition;
    private int _selectedSwitchToIndex = 0;

    public Battler SelectedMember { get; private set; }
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
        if (PlayerController.Instance != null)
        {
            _playerParty = PlayerController.Instance.GetComponent<BattleParty>();
            if (_playerParty == null)
            {
                Debug.LogError("BattleParty component missing on PlayerController.");
            }
        }
        else
        {
            Debug.LogError("PlayerController instance is missing.");
        }
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _prevState = _gameController.StateMachine.GetPrevState();
        SelectedMember = null;

        if (_partyScreen == null)
        {
            Debug.LogError("PartyScreen reference is missing.");
            return;
        }

        _partyScreen.SetPartyData();

        // Display context-sensitive information based on the previous state.
        if (_prevState == InventoryState.Instance && InventoryState.Instance.SelectedItem is SkillBook)
        {
            _partyScreen.ShowSkillBookUsability(InventoryState.Instance.SelectedItem as SkillBook);
        }
        else if (_prevState == BattleState.Instance)
        {
            BattleState battleState = _prevState as BattleState;
            _partyScreen.UpdateBattleIndicators(battleState.BattleSystem);
        }

        _partyScreen.gameObject.SetActive(true);
        _partyScreen.OnSelected += OnBattlerSelected;
        _partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        if (_partyScreen != null)
        {
            _partyScreen.HandleUpdate();
        }
    }

    public override void Exit()
    {
        if (_partyScreen != null)
        {
            _partyScreen.gameObject.SetActive(false);
            _partyScreen.ClearMessageText();
            _partyScreen.OnSelected -= OnBattlerSelected;
            _partyScreen.OnBack -= OnBack;
        }
    }

    private void OnBattlerSelected(int selection)
    {
        SelectedMember = _partyScreen.SelectedMember;
        _ = StartCoroutine(BattlerSelectedAction(selection));
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private IEnumerator BattlerSelectedAction(int selectedBattlerIndex)
    {
        // If coming from InventoryState, transition to UseItemState.
        if (_prevState == InventoryState.Instance)
        {
            yield return GoToUseItemState();
        }
        // If coming from BattleState, show a dynamic menu with options.
        else if (_prevState == BattleState.Instance)
        {
            BattleState battleState = _prevState as BattleState;

            DynamicMenuState.Instance.MenuItems = new List<string>
            {
                "Switch",
                "Summary",
                "Back"
            };
            yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);

            switch (DynamicMenuState.Instance.SelectedItem)
            {
                case 0: // Switch option
                    if (SelectedMember.Hp <= 0)
                    {
                        yield return DisplayTemporaryMessage($"{SelectedMember.Base.Name} is unable to fight!", "Choose a party member!");
                        yield break;
                    }
                    if (battleState.BattleSystem.PlayerUnits.Any(u => u.Battler == SelectedMember))
                    {
                        yield return DisplayTemporaryMessage($"{SelectedMember.Base.Name} is already in battle!", "Choose a party member!");
                        yield break;
                    }
                    if (battleState.BattleSystem.UnableToSwitch(SelectedMember))
                    {
                        yield return DisplayTemporaryMessage($"{SelectedMember.Base.Name} is already preparing for battle!", "Choose a party member!");
                        yield break;
                    }
                    _partyScreen.ResetSelection();
                    _gameController.StateMachine.Pop();
                    break;

                case 1: // Summary option
                    SummaryState.Instance.SelectedBattlerIndex = selectedBattlerIndex;
                    yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);
                    _partyScreen.SetSelectedIndex(SummaryState.Instance.SelectedBattlerIndex);
                    break;

                default:
                    yield break;
            }
        }
        // If coming from other states, handle switching or showing summary.
        else
        {
            if (_isSwitchingPosition)
            {
                if (_selectedSwitchToIndex == selectedBattlerIndex)
                {
                    yield return DisplayTemporaryMessage("You can't switch with the same party member!",
                        $"Choose a party member to switch with {_playerParty.Battlers[selectedBattlerIndex].Base.Name}.");
                    yield break;
                }

                // Perform the position switch.
                _isSwitchingPosition = false;
                (_playerParty.Battlers[selectedBattlerIndex], _playerParty.Battlers[_selectedSwitchToIndex]) =
                    (_playerParty.Battlers[_selectedSwitchToIndex], _playerParty.Battlers[selectedBattlerIndex]);
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

            switch (DynamicMenuState.Instance.SelectedItem)
            {
                case 0: // Summary option
                    SummaryState.Instance.SelectedBattlerIndex = selectedBattlerIndex;
                    yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);
                    _partyScreen.SetSelectedIndex(SummaryState.Instance.SelectedBattlerIndex);
                    break;

                case 1: // Switch option
                    if (_playerParty.Battlers.Count == 1)
                    {
                        yield return DisplayTemporaryMessage("There are no other party members to switch with!", "Choose a party member!");
                        yield break;
                    }
                    _isSwitchingPosition = true;
                    _selectedSwitchToIndex = selectedBattlerIndex;
                    _partyScreen.SaveSelection();
                    _partyScreen.SetMessageText($"Choose a party member to switch with {_playerParty.Battlers[selectedBattlerIndex].Base.Name}.");
                    break;

                default:
                    yield break;
            }
        }
    }

    private IEnumerator GoToUseItemState()
    {
        yield return _gameController.StateMachine.PushAndWait(UseItemState.Instance);
        _gameController.StateMachine.Pop();
    }

    private IEnumerator DisplayTemporaryMessage(string initialMessage, string followUpMessage)
    {
        _partyScreen.SetMessageText(initialMessage);
        _partyScreen.EnableInput(false);
        yield return new WaitForSeconds(1.25f);
        _partyScreen.SetMessageText(followUpMessage);
        _partyScreen.EnableInput(true);
    }

    private void OnBack()
    {
        if (_isSwitchingPosition)
        {
            _isSwitchingPosition = false;
            _partyScreen.RestoreSelection();
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _partyScreen.SetMessageText("Choose a party member!");
            return;
        }

        SelectedMember = null;
        State<GameController> prevState = _gameController.StateMachine.GetPrevState();

        if (prevState == BattleState.Instance)
        {
            BattleState battleState = prevState as BattleState;
            if (battleState.BattleSystem.PlayerUnits.Any(static u => u.Battler.Hp <= 0))
            {
                _partyScreen.SetMessageText("You have to choose a party member!");
                return;
            }
            _partyScreen.gameObject.SetActive(false);
        }
        _partyScreen.ResetSelection();
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        _gameController.StateMachine.Pop();
    }
}