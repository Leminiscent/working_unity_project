using System.Collections;
using UnityEngine;

public class TeleportObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToTeleport;
    [SerializeField] private Vector2 _destination;

    public override IEnumerator Play()
    {
        if (_objectToTeleport == null)
        {
            Debug.LogWarning("Object to teleport is not assigned in TeleportObjectAction.");
            yield break;
        }

        if (_destination == null)
        {
            Debug.LogWarning("Destination is not assigned in TeleportObjectAction.");
            yield break;
        }

        _objectToTeleport.transform.position = _destination;
        yield break;
    }
}