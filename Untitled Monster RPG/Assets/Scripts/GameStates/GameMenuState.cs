using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class GameMenuState : State<GameController>
{
    GameController gameController;

    public static GameMenuState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            gameController.StateMachine.Pop();
        }
    }
}
