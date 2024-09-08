using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI playerInventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public event Action OnStart;
    public event Action OnFinish;

    ShopState state;
    Merchant merchant;

    public static ShopController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    Inventory playerInventory;

    private void Start()
    {
        playerInventory = Inventory.GetInventory();
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        this.merchant = merchant;

        OnStart?.Invoke();
        yield return StartMenu();
    }

    IEnumerator StartMenu()
    {
        state = ShopState.Menu;

        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText("Welcome to my shop! How can I help you today?",
            waitForInput: false,
            choices: new List<string> { "Buy", "Sell", "Leave" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            // Buy
            state = ShopState.Buying;
            walletUI.Show();
            shopUI.Show(merchant.ItemsForSale, (item) => StartCoroutine(BuyItem(item)), OnBackFromBuying);
        }
        else if (selectedChoice == 1)
        {
            // Sell
            state = ShopState.Selling;
            playerInventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            // Leave
            OnFinish?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            playerInventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
        else if (state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }

    public void OnBackFromSelling()
    {
        playerInventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenu());
    }

    public void OnBackFromBuying()
    {
        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenu());
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;
        if (!item.IsSellable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("I'm sorry, I can't buy this item from you.");
            state = ShopState.Selling;
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
        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;
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
        state = ShopState.Buying;
    }
}
