using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueAction : CutsceneAction
{
    [SerializeField] private Dialogue _dialogue;

    public override IEnumerator Play()
    {
        if (_dialogue == null)
        {
            Debug.LogWarning("Dialogue is not assigned in DialogueAction.");
            yield break;
        }

        yield return DialogueManager.Instance.ShowDialogue(_dialogue);
    }
}