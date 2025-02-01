using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Utils.StateMachine;

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
        StartCoroutine(StartMenu());
    }

    private IEnumerator StartMenu()
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
            yield return _gameController.StateMachine.PushAndWait(ShopBuyingState.Instance);
            StartCoroutine(StartMenu());
        }
        else if (selectedChoice == 1)
        {
            // Sell
            yield return _gameController.StateMachine.PushAndWait(ShopSellingState.Instance);
            StartCoroutine(StartMenu());
        }
        else
        {
            // Leave
            yield return DialogueManager.Instance.ShowDialogueText("Thank you for visiting my shop! Come back soon!", true);
            _gameController.StateMachine.Pop();
        }
    }
}
