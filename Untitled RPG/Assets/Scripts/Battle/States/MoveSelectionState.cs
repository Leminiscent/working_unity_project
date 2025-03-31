using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.StateMachine;

public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] private MoveSelectionUI _selectionUI;
    [SerializeField] private GameObject _moveDetailsUI;

    private BattleSystem _battleSystem;

    public List<Move> Moves { get; set; } // List of moves available to the selecting unit
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

        // Validate UI references.
        if (_selectionUI == null)
        {
            Debug.LogError("MoveSelectionUI is not assigned in the inspector.");
            return;
        }
        if (_moveDetailsUI == null)
        {
            Debug.LogError("MoveDetailsUI is not assigned in the inspector.");
            return;
        }

        // Set the moves for the selection UI.
        _selectionUI.SetMoves(Moves);

        // If no move has sufficient SP, auto-select a backup move.
        if (Moves.Count(static m => m.Sp > 0) == 0)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.Fight,
                SelectedMove = new Move(GlobalSettings.Instance.BackupMove),
                TargetUnits = new List<BattleUnit>
                {
                    _battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]
                }
            });
            return;
        }

        // Activate UI elements and subscribe to UI events.
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
        if (_selectionUI != null)
        {
            _selectionUI.gameObject.SetActive(false);
            _selectionUI.OnSelected -= OnMoveSelected;
            _selectionUI.OnBack -= OnBack;
            _selectionUI.ClearItems();
        }
        if (_moveDetailsUI != null)
        {
            _moveDetailsUI.SetActive(false);
        }
        _battleSystem.DialogueBox.EnableDialogueText(true);
    }

    private void OnMoveSelected(int selection)
    {
        _ = StartCoroutine(OnMoveSelectedAsync(selection));
    }

    private IEnumerator OnMoveSelectedAsync(int selection)
    {
        Move selectedMove = Moves[selection];
        AudioManager.Instance.PlaySFX(AudioID.UISelect);

        // For moves with auto-targeting (Self, AllAllies, AllEnemies, Others), determine targets automatically.
        if (selectedMove.Base.Target is MoveTarget.Self or MoveTarget.AllAllies or MoveTarget.AllEnemies or MoveTarget.Others)
        {
            List<BattleUnit> targets = GetAutoTargetUnits(selectedMove);
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.Fight,
                SelectedMove = selectedMove,
                TargetUnits = targets
            });
            yield break;
        }

        // For moves that require manual target selection.
        int selectedTargetIndex = 0;
        if ((selectedMove.Base.Target is MoveTarget.Enemy && _battleSystem.EnemyUnits.Count > 1) ||
            (selectedMove.Base.Target is MoveTarget.Ally && _battleSystem.PlayerUnits.Count > 1))
        {
            TargetSelectionState.Instance.IsTargetingAllies = selectedMove.Base.Target is MoveTarget.Ally;
            yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
            if (!TargetSelectionState.Instance.SelectionMade)
            {
                yield break;
            }
            selectedTargetIndex = TargetSelectionState.Instance.SelectedTarget;
        }

        // Determine the target unit based on the manual selection.
        List<BattleUnit> selectedTargets = GetSelectedTargetUnits(selectedMove, selectedTargetIndex);
        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Fight,
            SelectedMove = selectedMove,
            TargetUnits = selectedTargets
        });
    }

    private List<BattleUnit> GetAutoTargetUnits(Move move)
    {
        if (move.Base.Target is MoveTarget.Self)
        {
            return new List<BattleUnit> { _battleSystem.SelectingUnit };
        }
        else if (move.Base.Target is MoveTarget.AllAllies)
        {
            return _battleSystem.PlayerUnits;
        }
        else if (move.Base.Target is MoveTarget.AllEnemies)
        {
            return _battleSystem.EnemyUnits;
        }
        else if (move.Base.Target is MoveTarget.Others)
        {
            return _battleSystem.PlayerUnits.Where(u => u != _battleSystem.SelectingUnit)
                   .Concat(_battleSystem.EnemyUnits)
                   .ToList();
        }
        return new List<BattleUnit>();
    }

    private List<BattleUnit> GetSelectedTargetUnits(Move move, int selectedIndex)
    {
        return move.Base.Target is MoveTarget.Enemy
            ? new List<BattleUnit> { _battleSystem.EnemyUnits[selectedIndex] }
            : new List<BattleUnit> { _battleSystem.PlayerUnits[selectedIndex] };
    }

    private void OnBack()
    {
        _battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
    }
}