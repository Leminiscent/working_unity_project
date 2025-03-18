using System.Collections;
using UnityEngine;

public class FadeOutAction : CutsceneAction
{
    [SerializeField] private float _duration = 0.5f;

    public override IEnumerator Play()
    {
        if (Fader.Instance == null)
        {
            Debug.LogWarning("Fader instance is not assigned.");
            yield break;
        }

        yield return Fader.Instance.FadeOut(_duration);
    }
}