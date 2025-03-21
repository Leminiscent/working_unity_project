using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] private Vector2 _shopCameraOffset;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private WalletUI _walletUI;
    [SerializeField] private CountSelectorUI _countSelectorUI;

    private GameController _gameController;
    private Inventory _playerInventory;
    private bool _isBrowsing;
    private bool _isProcessingBack;

    public List<ItemBase> AvailableItems { get; set; }
    public static ShopBuyingState Instance { get; private set; }

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
        _isBrowsing = false;
        _isProcessingBack = false;
        _ = StartCoroutine(StartBuying());
    }

    public override void Execute()
    {
        if (_isBrowsing)
        {
            _shopUI.HandleUpdate();
        }
    }

    private IEnumerator StartBuying()
    {
        yield return _gameController.MoveCamera(_shopCameraOffset);
        _walletUI.Show();
        _shopUI.Show(AvailableItems,
            item => StartCoroutine(BuyItem(item)),
            OnBackButtonPressed);
        _isBrowsing = true;
    }

    private void OnBackButtonPressed()
    {
        if (!_isProcessingBack)
        {
            _isProcessingBack = true;
            _ = StartCoroutine(HandleBackFromBuying());
        }
    }

    private IEnumerator BuyItem(ItemBase item)
    {
        _isBrowsing = false;

        // Ask the player how many items they want to buy.
        yield return DialogueManager.Instance.ShowDialogueText(
            $"How many {TextUtil.GetPlural(item.Name)} would you like?",
            waitForInput: false,
            autoClose: false);

        int countToBuy = 1;
        yield return _countSelectorUI.ShowSelector(99, item.Price,
            selectedCount => countToBuy = selectedCount);

        DialogueManager.Instance.CloseDialogue();

        // If the user cancels the purchase (selects 0), resume browsing.
        if (countToBuy == 0)
        {
            _isBrowsing = true;
            yield break;
        }

        int totalPrice = item.Price * countToBuy;

        // Check if the wallet has enough money.
        if (Wallet.Instance.HasEnoughMoney(totalPrice))
        {
            // Confirm the purchase with the player.
            int selectedChoice = -1;
            yield return DialogueManager.Instance.ShowDialogueText(
                $"That will be {TextUtil.GetNumText(totalPrice)} gold. Do we have a deal?",
                waitForInput: false,
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            // If the player confirms the purchase.
            if (selectedChoice == 0)
            {
                Wallet.Instance.SpendMoney(totalPrice);
                _playerInventory.AddItem(item, countToBuy);
                yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
            }
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText("You don't have enough money for that.");
        }
        _isBrowsing = true;
    }

    private IEnumerator HandleBackFromBuying()
    {
        _shopUI.EnableInput(false);
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        yield return _gameController.MoveCamera(-_shopCameraOffset);
        _shopUI.Close();
        _walletUI.Close();
        _gameController.StateMachine.Pop();
    }
}