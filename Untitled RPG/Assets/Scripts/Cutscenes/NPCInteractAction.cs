using System.Collections;
using UnityEngine;

public class NPCInteractAction : CutsceneAction
{
    [SerializeField] private NPCController _npc;

    public override IEnumerator Play()
    {
        yield return _npc.Interact(PlayerController.Instance.transform);
    }
}
