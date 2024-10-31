using System.Collections;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int count = 1;
    [SerializeField] private Dialogue dialogue;
    private bool used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        player.GetComponent<Inventory>().AddItem(item, count);
        used = true;
        AudioManager.Instance.PlaySFX(AudioID.ItemObtained, pauseMusic: true);

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

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
