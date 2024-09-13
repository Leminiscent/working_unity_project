using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] Sprite usedSprite;
    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);
            Used = true;
            GetComponent<SpriteRenderer>().sprite = usedSprite;

            string playerName = initiator.GetComponent<PlayerController>().Name;

            AudioManager.Instance.PlaySFX(AudioID.ItemObtained, pauseMusic: true);
            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} found {item.Name}!");
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
            GetComponent<SpriteRenderer>().sprite = usedSprite;
        }
    }
}
