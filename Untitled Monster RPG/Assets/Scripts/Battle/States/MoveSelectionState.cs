using System.Collections;
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
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.Fight,
                SelectedMove = new Move(GlobalSettings.Instance.BackupMove),
                TargetUnits = _battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]
            });
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
        StartCoroutine(OnMoveSelectedAsync(selection));
    }

    private IEnumerator OnMoveSelectedAsync(int selection)
    {
        Move selectedMove = Moves[selection];

        if (selectedMove.Base.Target == MoveTarget.Self)
        {
            _battleSystem.AddBattleAction(new BattleAction()
            {
                ActionType = BattleActionType.Fight,
                SelectedMove = selectedMove,
                TargetUnits = _battleSystem.SelectingUnit
            });
            yield break;
        }

        int moveTarget = 0;
        if (_battleSystem.EnemyUnits.Count > 1)
        {
            yield return _battleSystem.StateMachine.PushAndWait(TargetSelectionState.Instance);
            if (!TargetSelectionState.Instance.SelectionMade)
            {
                yield break;
            }
            moveTarget = TargetSelectionState.Instance.SelectedTarget;
        }

        _battleSystem.AddBattleAction(new BattleAction()
        {
            ActionType = BattleActionType.Fight,
            SelectedMove = selectedMove,
            TargetUnits = _battleSystem.EnemyUnits[moveTarget]
        });
    }

    private void OnBack()
    {
        _battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
    }
}
