using System.Collections;
using UnityEngine;

public class FadeOutAction : CutsceneAction
{
    [SerializeField] private float duration = 0.5f;

    public override IEnumerator Play()
    {
        yield return Fader.Instance.FadeOut(duration);
    }
}
