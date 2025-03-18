using System.Collections;
using UnityEngine;

public class DisableObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToDisable;

    public override IEnumerator Play()
    {
        if (_objectToDisable == null)
        {
            Debug.LogWarning("Object to disable is not assigned in DisableObjectAction.");
            yield break;
        }

        _objectToDisable.SetActive(false);
        yield break;
    }
}