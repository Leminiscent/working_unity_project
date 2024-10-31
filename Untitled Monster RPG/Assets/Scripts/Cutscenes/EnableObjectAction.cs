using System.Collections;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] private GameObject objectToEnable;

    public override IEnumerator Play()
    {
        objectToEnable.SetActive(true);
        yield break;
    }
}
