using UnityEngine;
using Utils.StateMachine;

public class FreeRoamState : State<GameController>
{
    private GameController _gameController;

    public static FreeRoamState Instance { get; private set; }

    public void Awake()
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
    }

    public override void Execute()
    {
        PlayerController.Instance.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            _gameController.StateMachine.Push(GameMenuState.Instance);
        }
    }
}
