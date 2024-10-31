using System.Collections;
using UnityEngine;

public class TeleportObjectAction : CutsceneAction
{
    [SerializeField] GameObject objectToTeleport;
    [SerializeField] Vector2 destination;

    public override IEnumerator Play()
    {
        objectToTeleport.transform.position = destination;
        yield break;
    }
}
