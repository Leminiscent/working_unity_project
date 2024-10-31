using System.Collections;
using UnityEngine;

[System.Serializable]
public class DialogueAction : CutsceneAction
{
    [SerializeField] Dialogue dialogue;

    public override IEnumerator Play()
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
    }
}
