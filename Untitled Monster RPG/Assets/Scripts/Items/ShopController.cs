using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }

public class ShopController : MonoBehaviour
{
    [SerializeField] InventoryUI playerInventoryUI;
    ShopState state = ShopState.Menu;

    public static ShopController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
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
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if (state == ShopState.Selling)
        {
            playerInventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) => { });
        }
    }

    public void OnBackFromSelling()
    {
        playerInventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenu());
    }
}
