using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.StateMachine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] private WalletUI _walletUI;
    [SerializeField] private CountSelectorUI _countSelectorUI;

    private GameController _gameController;
    private Inventory _playerInventory;

    public static ShopSellingState Instance { get; private set; }

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

    private void Start()
    {
        _playerInventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _ = StartCoroutine(HandleSelling());
    }

    private IEnumerator HandleSelling()
    {
        while (true)
        {
            yield return Fader.Instance.FadeIn(0.5f);

            // Push the InventoryState to allow the player to select an item to sell.
            _gameController.StateMachine.Push(InventoryState.Instance);

            _gameController.StateMachine.Push(CutsceneState.Instance);
            yield return Fader.Instance.FadeOut(0.5f);

            _gameController.StateMachine.Pop();
            yield return new WaitUntil(() => _gameController.StateMachine.CurrentState == Instance);

            ItemBase selectedItem = InventoryState.Instance.SelectedItem;

            // If no item is selected, exit the selling state.
            if (selectedItem == null)
            {
                _gameController.StateMachine.Pop();
                yield break;
            }

            // Process the sale for the selected item.
            yield return SellItem(selectedItem);
        }
    }

    private IEnumerator SellItem(ItemBase item)
    {
        // Check if the item is sellable.
        if (!item.IsSellable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("I'm sorry, I can't buy this item from you.");
            yield break;
        }

        _walletUI.Show();

        // Calculate the base selling price (50% of original price, rounded).
        int sellingPrice = (int)Mathf.Round(item.Price * 0.5f);
        int countToSell = 1;
        int availableCount = _playerInventory.GetItemCount(item);

        // If more than one item is available, ask the player how many to sell.
        if (availableCount > 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText(
                $"How many {TextUtil.GetPlural(item.Name)} would you like to sell?",
                waitForInput: false,
                autoClose: false);

            yield return _countSelectorUI.ShowSelector(availableCount, sellingPrice,
                selectedCount => countToSell = selectedCount);

            yield return DialogueManager.Instance.CloseDialogue();
        }

        // If the player cancels the sale, close the wallet UI and exit.
        if (countToSell == 0)
        {
            _walletUI.Close();
            yield break;
        }

        // Calculate the total selling price.
        int totalSellingPrice = sellingPrice * countToSell;

        int selectedChoice = -1;
        yield return DialogueManager.Instance.ShowDialogueText(
            $"I can buy {TextUtil.GetNumText(countToSell)} {TextUtil.GetPlural(item.Name, countToSell)} from you for {totalSellingPrice} GP. Do we have a deal?",
            waitForInput: false,
            choices: new List<string> { "Yes", "No" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        // If the player confirms the sale, remove the items and add money to the wallet.
        if (selectedChoice == 0)
        {
            _playerInventory.RemoveItem(item, countToSell);
            Wallet.Instance.AddMoney(totalSellingPrice);
            AudioManager.Instance.PlaySFX(AudioID.ItemObtained);
            yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
        }
        _walletUI.Close();
    }
}