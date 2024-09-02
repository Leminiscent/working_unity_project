using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialogue dialogue;

    bool used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        player.GetComponent<Inventory>().AddItem(item, count);
        used = true;

        string dialogueText = $"{player.Name} received {item.Name}!";

        if (count > 1)
        {
            dialogueText = $"{player.Name} received {count} {item.Name}s!";
        }
        yield return DialogueManager.Instance.ShowDialogueText(dialogueText);
    }

    public bool CanBeGiven()
    {
        return item != null && count > 0 && !used;
    }
}
