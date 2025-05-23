using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableTree : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemBase _requiredItem;

    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogueManager.Instance.ShowDialogueText("This tree looks like it can be cut down.");

        Inventory inventory = initiator.GetComponent<Inventory>();
        if (inventory != null && inventory.HasItem(_requiredItem))
        {
            int selectedChoice = 0;
            PlayerController playerController = initiator.GetComponent<PlayerController>();
            string playerName = playerController != null ? playerController.Name : "Player";

            yield return DialogueManager.Instance.ShowDialogueText(
                "Would you like to cut down the tree?",
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                yield return ObjectUtil.ScaleOut(gameObject, 0.05f);
                yield return DialogueManager.Instance.ShowDialogueText($"{playerName} cut down the tree! The path forward has been cleared!");
            }
        }
    }
}