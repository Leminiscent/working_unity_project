using System.Collections;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] GameObject objectToEnable;

    public override IEnumerator Play()
    {
        objectToEnable.SetActive(true);
        yield break;
    }
}
