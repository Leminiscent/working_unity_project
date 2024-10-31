using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] InventoryUI playerInventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    GameController gameController;
    Inventory playerInventory;

    public List<ItemBase> AvailableItems { get; set; }
    public static ShopSellingState Instance { get; private set; }

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
        StartCoroutine(StartSelling());
    }

    IEnumerator StartSelling()
    {
        yield return gameController.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;

        if (selectedItem != null)
        {
            yield return SellItem(selectedItem);
            StartCoroutine(StartSelling());
        }
        else
        {
            gameController.StateMachine.Pop();
        }
    }

    IEnumerator SellItem(ItemBase item)
    {
        if (!item.IsSellable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("I'm sorry, I can't buy this item from you.");
            yield break;
        }
        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price * 0.5f);
        int countToSell = 1;
        int itemCount = playerInventory.GetItemCount(item);

        if (itemCount > 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText($"How many {item.Name} would you like to sell?",
                waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                selectedCount => countToSell = selectedCount);

            DialogueManager.Instance.CloseDialogue();
        }
        sellingPrice *= countToSell;

        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText($"I can buy this {item.Name} from you for {sellingPrice} gold. Do we have a deal?",
            waitForInput: false,
            choices: new List<string> { "Yes", "No" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            playerInventory.RemoveItem(item, countToSell);
            Wallet.Instance.AddMoney(sellingPrice);
            yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
        }
        walletUI.Close();
    }
}
