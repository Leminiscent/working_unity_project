using System.Collections;
using UnityEngine;

public class TeleportObjectAction : CutsceneAction
{
    [SerializeField] private GameObject objectToTeleport;
    [SerializeField] private Vector2 destination;

    public override IEnumerator Play()
    {
        objectToTeleport.transform.position = destination;
        yield break;
    }
}
