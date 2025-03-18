using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] private Dialogue _dialogue;

    public bool TriggerRepeatedly => false;

    public void OnPlayerTriggered(PlayerController player)
    {
        if (DialogueManager.Instance != null)
        {
            player.Character.Animator.IsMoving = false;
            _ = StartCoroutine(DialogueManager.Instance.ShowDialogue(_dialogue));
        }
        else
        {
            Debug.LogWarning("DialogueManager instance not found.");
        }
    }
}