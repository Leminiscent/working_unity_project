using System.Collections;
using UnityEngine;

public class FadeInAction : CutsceneAction
{
    [SerializeField] float duration = 0.5f;

    public override IEnumerator Play()
    {
        yield return Fader.Instance.FadeIn(duration);
    }
}
