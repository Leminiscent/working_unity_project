using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ForgettingMoveState : State<GameController>
{
    [SerializeField] private MoveForgettingUI _moveForgettingUI;

    private GameController _gameController;

    public List<MoveBase> CurrentMoves { get; set; }
    public MoveBase NewMove { get; set; }
    public int Selection { get; set; }
    public static ForgettingMoveState Instance { get; private set; }

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

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        Selection = 0;
        _moveForgettingUI.gameObject.SetActive(true);
        _moveForgettingUI.SetMoveData(CurrentMoves, NewMove);
        _moveForgettingUI.OnSelected += OnMoveSelected;
        _moveForgettingUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        _moveForgettingUI.HandleUpdate();
    }

    public override void Exit()
    {
        _moveForgettingUI.gameObject.SetActive(false);
        _moveForgettingUI.OnSelected -= OnMoveSelected;
        _moveForgettingUI.OnBack -= OnBack;
    }

    private void OnMoveSelected(int selection)
    {
        _moveForgettingUI.ResetSelection();
        Selection = selection;
        _gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        _moveForgettingUI.ResetSelection();
        Selection = -1;
        _gameController.StateMachine.Pop();
    }
}
