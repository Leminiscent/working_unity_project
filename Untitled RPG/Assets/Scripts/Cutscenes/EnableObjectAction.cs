using System.Collections;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToEnable;

    public override IEnumerator Play()
    {
        _objectToEnable.SetActive(true);
        yield break;
    }
}
