using System.Collections;
using System.Collections.Generic;
using Util.StateMachine;

public class ShopMenuState : State<GameController>
{
    private GameController _gameController;

    public List<ItemBase> AvailableItems { get; set; }
    public static ShopMenuState Instance { get; private set; }

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
        _ = StartCoroutine(ShowMenu());
    }

    private IEnumerator ShowMenu()
    {
        // Display the shop menu and wait for the player to make a choice.
        bool isFirstIteration = true;
        while (true)
        {
            int menuChoice = -1;

            string promptText = isFirstIteration
                ? "Welcome to my shop! How can I help you today?"
                : "Is there anything else I can help you with?";
            isFirstIteration = false;

            // Display the shop menu dialogue and capture the player's choice.
            yield return DialogueManager.Instance.ShowDialogueText(
                promptText,
                waitForInput: false,
                choices: new List<string> { "Buy", "Sell", "Leave" },
                onChoiceSelected: choiceIndex => menuChoice = choiceIndex
            );

            if (menuChoice == 0) // Buy
            {
                ShopBuyingState.Instance.AvailableItems = AvailableItems;
                // Push the buying state and wait for it to complete before re-displaying the menu.
                yield return _gameController.StateMachine.PushAndWait(ShopBuyingState.Instance);
            }
            else if (menuChoice == 1) // Sell
            {
                yield return _gameController.StateMachine.PushAndWait(ShopSellingState.Instance);
            }
            else if (menuChoice == 2) // Leave
            {
                yield return DialogueManager.Instance.ShowDialogueText("Thank you for visiting my shop! Come back soon!");
                _gameController.StateMachine.Pop();
                break;
            }
        }
    }
}