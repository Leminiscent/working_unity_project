using System.Collections;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private int _count = 1;
    [SerializeField] private Dialogue _dialogue;

    private bool _used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(_dialogue);

        player.GetComponent<Inventory>().AddItem(_item, _count);

        _used = true;
        AudioManager.Instance.PlaySFX(AudioID.ItemObtained, pauseMusic: true);

        // Build the dialogue text based on the count of items given.
        string dialogueText = _count > 1
            ? $"{player.Name} received {_count} {_item.Name}s!"
            : $"{player.Name} received {_item.Name}!";

        yield return DialogueManager.Instance.ShowDialogueText(dialogueText);
    }

    public bool CanBeGiven()
    {
        return _item != null && _count > 0 && !_used;
    }

    public object CaptureState()
    {
        return _used;
    }

    public void RestoreState(object state)
    {
        _used = (bool)state;
    }
}