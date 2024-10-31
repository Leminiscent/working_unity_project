using System.Collections;
using UnityEngine;

public class TeleportObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToTeleport;
    [SerializeField] private Vector2 _destination;

    public override IEnumerator Play()
    {
        _objectToTeleport.transform.position = _destination;
        yield break;
    }
}
