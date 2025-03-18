using System.Collections;
using UnityEngine;

public class NPCInteractAction : CutsceneAction
{
    [SerializeField] private NPCController _npc;

    public override IEnumerator Play()
    {
        if (_npc == null)
        {
            Debug.LogWarning("NPC is not assigned in NPCInteractAction.");
            yield break;
        }

        yield return _npc.Interact(PlayerController.Instance.transform);
    }
}