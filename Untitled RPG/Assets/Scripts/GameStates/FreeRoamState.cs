using System.Collections;
using UnityEngine;
using Util.StateMachine;

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
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.HandleUpdate();
        }
        else
        {
            Debug.LogError("PlayerController instance is missing in FreeRoamState.");
        }

        if (Input.GetButtonDown("Submit"))
        {
            // Initiate the transition to the game menu.
            _ = StartCoroutine(OpenMenu());
        }
    }

    public IEnumerator OpenMenu()
    {
        if (PlayerController.Instance != null &&
            PlayerController.Instance.Character != null &&
            PlayerController.Instance.Character.Animator != null)
        {
            PlayerController.Instance.Character.Animator.IsMoving = false;
        }
        else
        {
            Debug.LogWarning("PlayerController, Character, or Animator reference is missing.");
        }

        yield return _gameController.StateMachine.PushAndWait(GameMenuState.Instance);
    }
}