using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ForgettingMoveState : State<GameController>
{
    [SerializeField] MoveForgettingUI moveForgettingUI;
    GameController gameController;

    public List<MoveBase> CurrentMoves { get; set; }
    public MoveBase NewMove { get; set; }
    public int Selection { get; set; }
    public static ForgettingMoveState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        Selection = 0;
        moveForgettingUI.gameObject.SetActive(true);
        moveForgettingUI.SetMoveData(CurrentMoves, NewMove);
        moveForgettingUI.OnSelected += OnMoveSelected;
        moveForgettingUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        moveForgettingUI.HandleUpdate();
    }

    public override void Exit()
    {
        moveForgettingUI.gameObject.SetActive(false);
        moveForgettingUI.OnSelected -= OnMoveSelected;
        moveForgettingUI.OnBack -= OnBack;
    }

    void OnMoveSelected(int selection)
    {
        Selection = selection;
        gameController.StateMachine.Pop();
    }

    void OnBack()
    {
        Selection = -1;
        gameController.StateMachine.Pop();
    }
}
