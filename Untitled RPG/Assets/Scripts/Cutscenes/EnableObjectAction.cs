using System.Collections;
using UnityEngine;

public class EnableObjectAction : CutsceneAction
{
    [SerializeField] private GameObject _objectToEnable;

    public override IEnumerator Play()
    {
        if (_objectToEnable == null)
        {
            Debug.LogWarning("Object to enable is not assigned in EnableObjectAction.");
            yield break;
        }

        _objectToEnable.SetActive(true);
        yield break;
    }
}