using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private Sprite _usedSprite;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(_item);
            Used = true;
            GetComponent<SpriteRenderer>().sprite = _usedSprite;

            string playerName = initiator.GetComponent<PlayerController>().Name;

            AudioManager.Instance.PlaySFX(AudioID.ItemObtained, pauseMusic: true);
            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} found {_item.Name}!");
        }
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;
        if (Used)
        {
            GetComponent<SpriteRenderer>().sprite = _usedSprite;
        }
    }
}
