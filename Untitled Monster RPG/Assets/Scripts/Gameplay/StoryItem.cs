using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private Dialogue dialogue;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(DialogueManager.Instance.ShowDialogue(dialogue));
    }

    public bool TriggerRepeatedly => false;
}
