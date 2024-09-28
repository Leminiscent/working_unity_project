using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopMenuState : State<GameController>
{
    GameController gameController;

    public List<ItemBase> AvailableItems { get; set; }
    public static ShopMenuState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        StartCoroutine(StartMenu());
    }

    IEnumerator StartMenu()
    {
        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText("Welcome to my shop! How can I help you today?",
            waitForInput: false,
            choices: new List<string> { "Buy", "Sell", "Leave" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Buy
            ShopBuyingState.Instance.AvailableItems = AvailableItems;
            yield return gameController.StateMachine.PushAndWait(ShopBuyingState.Instance);
        }
        else if (selectedChoice == 1)
        {
            // Sell
            yield return gameController.StateMachine.PushAndWait(ShopSellingState.Instance);
        }

        gameController.StateMachine.Pop();
    }
}
