using System.Collections;
using UnityEngine;

public class NPCInteractAction : CutsceneAction
{
    [SerializeField] NPCController npc;

    public override IEnumerator Play()
    {
        yield return npc.Interact(PlayerController.Instance.transform);
    }
}
