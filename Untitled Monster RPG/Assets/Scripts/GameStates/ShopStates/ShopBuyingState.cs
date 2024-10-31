using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] private Vector2 shopCameraOffest;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private WalletUI walletUI;
    [SerializeField] private CountSelectorUI countSelectorUI;
    private GameController gameController;
    private Inventory playerInventory;
    private bool browseItems;

    public List<ItemBase> AvailableItems { get; set; }
    public static ShopBuyingState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerInventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        browseItems = false;
        StartCoroutine(StartBuying());
    }

    public override void Execute()
    {
        if (browseItems)
        {
            shopUI.HandleUpdate();
        }
    }

    private IEnumerator StartBuying()
    {
        yield return GameController.Instance.MoveCamera(shopCameraOffest);
        walletUI.Show();
        shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)), () => StartCoroutine(OnBackFromBuying()));
        browseItems = true;
    }

    private IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;
        yield return DialogueManager.Instance.ShowDialogueText($"How many {item.Name}'s would you like?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;

        yield return countSelectorUI.ShowSelector(99, item.Price,
            selectedCount => countToBuy = selectedCount);

        DialogueManager.Instance.CloseDialogue();

        float totalPrice = item.Price * countToBuy;

        if (Wallet.Instance.HasEnoughMoney(totalPrice))
        {
            int selectedChoice = 0;

            yield return DialogueManager.Instance.ShowDialogueText($"That will be {totalPrice} gold. Do we have a deal?",
                waitForInput: false,
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0)
            {
                Wallet.Instance.SpendMoney(totalPrice);
                playerInventory.AddItem(item, countToBuy);
                yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
            }
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText("You don't have enough money for that.");
        }
        browseItems = true;
    }

    private IEnumerator OnBackFromBuying()
    {
        yield return GameController.Instance.MoveCamera(-shopCameraOffest);
        shopUI.Close();
        walletUI.Close();
        gameController.StateMachine.Pop();
    }
}
