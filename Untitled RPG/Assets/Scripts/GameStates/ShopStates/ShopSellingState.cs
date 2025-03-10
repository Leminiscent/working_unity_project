using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopSellingState : State<GameController>
{
    [SerializeField] private InventoryUI _playerInventoryUI;
    [SerializeField] private WalletUI _walletUI;
    [SerializeField] private CountSelectorUI _countSelectorUI;

    private GameController _gameController;
    private Inventory _playerInventory;

    public List<ItemBase> AvailableItems { get; set; }
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
        StartCoroutine(StartSelling());
    }

    private IEnumerator StartSelling()
    {
        yield return _gameController.StateMachine.PushAndWait(InventoryState.Instance);

        ItemBase selectedItem = InventoryState.Instance.SelectedItem;

        if (selectedItem != null)
        {
            yield return SellItem(selectedItem);
            StartCoroutine(StartSelling());
        }
        else
        {
            _gameController.StateMachine.Pop();
        }
    }

    private IEnumerator SellItem(ItemBase item)
    {
        if (!item.IsSellable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("I'm sorry, I can't buy this item from you.");
            yield break;
        }
        _walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price * 0.5f);
        int countToSell = 1;
        int itemCount = _playerInventory.GetItemCount(item);

        if (itemCount > 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText($"How many {item.Name}s would you like to sell?",
                waitForInput: false, autoClose: false);

            yield return _countSelectorUI.ShowSelector(itemCount, sellingPrice,
                selectedCount => countToSell = selectedCount);

            DialogueManager.Instance.CloseDialogue();
        }

        if (countToSell == 0)
        {
            _walletUI.Close();
            yield break;
        }

        sellingPrice *= countToSell;

        int selectedChoice = 0;
        
        yield return DialogueManager.Instance.ShowDialogueText($"I can buy {countToSell} {(countToSell > 1 ? $"{item.Name}s" : item.Name)} from you for {sellingPrice} gold. Do we have a deal?",
            waitForInput: false,
            choices: new List<string> { "Yes", "No" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            _playerInventory.RemoveItem(item, countToSell);
            Wallet.Instance.AddMoney(sellingPrice);
            yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
        }
        _walletUI.Close();
    }
}
