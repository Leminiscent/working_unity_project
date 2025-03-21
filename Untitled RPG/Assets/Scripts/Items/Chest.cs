using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private Sprite _usedSprite;

    private SpriteRenderer _spriteRenderer;

    public bool Used { get; set; } = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (Used)
        {
            yield break;
        }

        Inventory inventory = initiator.GetComponent<Inventory>();
        PlayerController playerController = initiator.GetComponent<PlayerController>();

        if (inventory != null && playerController != null)
        {
            inventory.AddItem(_item);
            Used = true;
            _spriteRenderer.sprite = _usedSprite;

            AudioManager.Instance.PlaySFX(AudioID.ItemObtained, pauseMusic: true);
            yield return DialogueManager.Instance.ShowDialogueText($"{playerController.Name} found {TextUtil.GetArticle(_item.Name)} {_item.Name}!");
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
            _spriteRenderer.sprite = _usedSprite;
        }
    }
}