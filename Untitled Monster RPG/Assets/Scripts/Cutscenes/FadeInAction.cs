using System.Collections;
using UnityEngine;

public class FadeInAction : CutsceneAction
{
    [SerializeField] private float _duration = 0.5f;

    public override IEnumerator Play()
    {
        yield return Fader.Instance.FadeIn(_duration);
    }
}
