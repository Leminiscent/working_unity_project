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

        string countText = TextUtil.ConvertNumToText(_count);
        yield return _count > 1
            ? DialogueManager.Instance.ShowDialogueText($"{player.Name} received {countText} {TextUtil.GetPluralizedNoun(_item.Name)}!")
            : DialogueManager.Instance.ShowDialogueText($"{player.Name} received {countText} {_item.Name}!");
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