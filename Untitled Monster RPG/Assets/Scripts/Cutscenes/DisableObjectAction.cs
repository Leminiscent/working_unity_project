using System.Collections;
using System.Collections.Generic;
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
