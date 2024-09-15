using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableTree : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase requiredItem;

    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogueManager.Instance.ShowDialogueText("This tree looks like it can be cut down.");
        if (initiator.GetComponent<Inventory>().HasItem(requiredItem))
        {
            int selectedChoice = 0;
            string playerName = initiator.GetComponent<PlayerController>().Name;

            yield return DialogueManager.Instance.ShowDialogueText("Would you like to cut down the tree?",
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                gameObject.SetActive(false);
                yield return DialogueManager.Instance.ShowDialogueText($"{playerName} cut down the tree! The path forward has been cleard!");
            }
        }
    }
}
