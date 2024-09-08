using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
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
        }
        else if (selectedChoice == 2)
        {
            // Leave
            yield break;
        }
    }
}
