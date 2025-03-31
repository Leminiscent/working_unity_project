using System.Collections.Generic;
using UnityEngine;
using Util.StateMachine;

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

        // Activate and initialize the UI.
        if (_moveForgettingUI != null)
        {
            StartCoroutine(ObjectUtil.ScaleIn(_moveForgettingUI.gameObject));
            _moveForgettingUI.SetMoveData(CurrentMoves, NewMove);
            _moveForgettingUI.OnSelected += OnMoveSelected;
            _moveForgettingUI.OnBack += OnBack;
        }
        else
        {
            Debug.LogError("MoveForgettingUI reference is missing.");
        }
    }

    public override void Execute()
    {
        if (_moveForgettingUI != null)
        {
            _moveForgettingUI.HandleUpdate();
        }
    }

    public override void Exit()
    {
        if (_moveForgettingUI != null)
        {
            StartCoroutine(ObjectUtil.ScaleOut(_moveForgettingUI.gameObject));
            _moveForgettingUI.OnSelected -= OnMoveSelected;
            _moveForgettingUI.OnBack -= OnBack;
        }
    }

    private void OnMoveSelected(int selection)
    {
        _moveForgettingUI.ResetSelection();
        Selection = selection;
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
        _gameController.StateMachine.Pop();
    }

    private void OnBack()
    {
        _moveForgettingUI.ResetSelection();
        Selection = -1;
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        _gameController.StateMachine.Pop();
    }
}