using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] private Vector2 _shopCameraOffest;
    [SerializeField] private ShopUI _shopUI;
    [SerializeField] private WalletUI _walletUI;
    [SerializeField] private CountSelectorUI _countSelectorUI;

    private GameController _gameController;
    private Inventory _playerInventory;
    private bool _browseItems;

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
        _browseItems = false;
        StartCoroutine(StartBuying());
    }

    public override void Execute()
    {
        if (_browseItems)
        {
            _shopUI.HandleUpdate();
        }
    }

    private IEnumerator StartBuying()
    {
        yield return GameController.Instance.MoveCamera(_shopCameraOffest);
        _walletUI.Show();
        _shopUI.Show(AvailableItems, (item) => StartCoroutine(BuyItem(item)), () => StartCoroutine(OnBackFromBuying()));
        _browseItems = true;
    }

    private IEnumerator BuyItem(ItemBase item)
    {
        _browseItems = false;
        yield return DialogueManager.Instance.ShowDialogueText($"How many {item.Name}'s would you like?",
            waitForInput: false, autoClose: false);

        int countToBuy = 1;

        yield return _countSelectorUI.ShowSelector(99, item.Price,
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
                _playerInventory.AddItem(item, countToBuy);
                yield return DialogueManager.Instance.ShowDialogueText("Thank you for your business!");
            }
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText("You don't have enough money for that.");
        }
        _browseItems = true;
    }

    private IEnumerator OnBackFromBuying()
    {
        yield return GameController.Instance.MoveCamera(-_shopCameraOffest);
        _shopUI.Close();
        _walletUI.Close();
        _gameController.StateMachine.Pop();
    }
}
