using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class CutsceneAction
{
    [field: SerializeField, FormerlySerializedAs("_actionName")] public string ActionName { get; set; }
    [field: SerializeField, FormerlySerializedAs("_waitForCompletion")] public bool WaitForCompletion { get; private set; } = true;

    public abstract IEnumerator Play();
}