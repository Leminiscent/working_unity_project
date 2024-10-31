using System.Collections;
using UnityEngine;

public class DisableObjectAction : CutsceneAction
{
    [SerializeField] GameObject objectToDisable;

    public override IEnumerator Play()
    {
        objectToDisable.SetActive(false);
        yield break;
    }
}
