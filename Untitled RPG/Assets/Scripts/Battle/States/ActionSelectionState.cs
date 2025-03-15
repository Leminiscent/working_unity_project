using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.StateMachine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] private ActionSelectionUI _selectionUI;

    private BattleSystem _battleSystem;
    private int _prevSelection = 0;
    private TextMeshProUGUI _talkText;

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
        _selectionUI.OnBack += OnBack;
        _battleSystem.DialogueBox.SetDialogue($"Choose an action for {_battleSystem.SelectingUnit.Battler.Base.Name}!");
        _battleSystem.SelectingUnit.SetSelected(true);

        _talkText = _selectionUI.GetComponentsInChildren<TextSlot>().ToList()[1].GetComponent<TextMeshProUGUI>();
        if (!_battleSystem.SelectingUnit.Battler.IsCommander)
        {
            _talkText.color = GlobalSettings.Instance.EmptyColor;
        }
        else
        {
            _talkText.color = Color.white;
        }
    }

    public override void Execute()
    {
        _selectionUI.HandleUpdate();

        if (!_battleSystem.SelectingUnit.Battler.IsCommander)
        {
            if (_selectionUI.SelectedIndex == 1)
            {
                int newIndex = _prevSelection == 0 ? 2
                    : _prevSelection == 2 ? 0
                    : 4;

                _selectionUI.SetSelectedIndex(newIndex);
            }

            _talkText.color = GlobalSettings.Instance.EmptyColor;
        }
        _prevSelection = _selectionUI.SelectedIndex;
    }

    public override void Exit()
    {
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnActionSelected;
        _selectionUI.OnBack -= OnBack;
    }

    private void OnActionSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                MoveSelectionState.Instance.Moves = _battleSystem.SelectingUnit.Battler.Moves;
                _battleSystem.StateMachine.ChangeState(MoveSelectionState.Instance);
                break;
            case 1:
                StartCoroutine(SelectRecruitTarget());
                break;
            case 2:
                StartCoroutine(SelectItemAndTarget());
                break;
            case 3:
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.Guard
                });
                break;
            case 4:
                StartCoroutine(GoToPartyState());
                break;
            case 5:
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.Run
                });
                break;
            default:
                break;
        }
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private IEnumerator SelectRecruitTarget()
    {
        int recruitTarget = 0;
        if (_battleSystem.EnemyUnits.Count > 1)
        {
            TargetSelectionState.Instance.IsTargetingAllies = false;
            yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
            if (!TargetSelectionState.Instance.SelectionMade)
            {
                yield break;
            }
            recruitTarget = TargetSelectionState.Instance.SelectedTarget;
        }
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Talk,
            TargetUnits = new List<BattleUnit> { _battleSystem.EnemyUnits[recruitTarget] }
        });
    }

    private IEnumerator SelectItemAndTarget()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;
        if (selectedItem != null)
        {
            if (selectedItem.Target is MoveTarget.Self or MoveTarget.AllAllies or MoveTarget.AllEnemies or MoveTarget.Others)
            {
                _battleSystem.AddBattleAction(new BattleAction()
                {
                    ActionType = BattleActionType.UseItem,
                    SelectedItem = selectedItem,
                    TargetUnits = selectedItem.Target is MoveTarget.Self ? new List<BattleUnit> { _battleSystem.SelectingUnit }
                        : selectedItem.Target is MoveTarget.AllAllies ? _battleSystem.PlayerUnits
                        : selectedItem.Target is MoveTarget.AllEnemies ? _battleSystem.EnemyUnits
                        : _battleSystem.PlayerUnits.Where(u => u != _battleSystem.SelectingUnit).Concat(_battleSystem.EnemyUnits).ToList()
                });
                yield break;
            }

            int itemTarget = 0;
            if ((selectedItem.Target is MoveTarget.Enemy && _battleSystem.EnemyUnits.Count > 1) || (selectedItem.Target is MoveTarget.Ally && _battleSystem.PlayerUnits.Count > 1))
            {
                TargetSelectionState.Instance.IsTargetingAllies = selectedItem.Target is MoveTarget.Ally;
                yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
                if (!TargetSelectionState.Instance.SelectionMade)
                {
                    yield break;
                }
                itemTarget = TargetSelectionState.Instance.SelectedTarget;
            }

            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.UseItem,
                SelectedItem = selectedItem,
                TargetUnits = selectedItem.Target is MoveTarget.Enemy ? new List<BattleUnit> { _battleSystem.EnemyUnits[itemTarget] }
                    : new List<BattleUnit> { _battleSystem.PlayerUnits[itemTarget] }
            });
        }
    }

    private IEnumerator GoToPartyState()
    {
        yield return GameController.Instance.StateMachine.PushAndWait(PartyState.Instance);

        Battler selectedBattler = PartyState.Instance.SelectedMember;
        if (selectedBattler != null)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.SwitchBattler,
                SelectedBattler = selectedBattler
            });
        }
    }

    private void OnBack()
    {
        _battleSystem.UndoBattleAction();
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
    }
}