using UnityEngine;
using Utils.StateMachine;

public class FreeRoamState : State<GameController>
{
    private GameController gameController;

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
        gameController = owner;
    }

    public override void Execute()
    {
        PlayerController.Instance.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            gameController.StateMachine.Push(GameMenuState.Instance);
        }
    }
}
