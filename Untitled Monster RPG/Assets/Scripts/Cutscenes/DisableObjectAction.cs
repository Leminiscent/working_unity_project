using System.Collections;
using UnityEngine;

public class DisableObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToDisable;

    public override IEnumerator Play()
    {
        _objectToDisable.SetActive(false);
        yield break;
    }
}
