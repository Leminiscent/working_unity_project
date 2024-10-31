using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueAction : CutsceneAction
{
    [SerializeField] private Dialogue dialogue;

    public override IEnumerator Play()
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
    }
}
