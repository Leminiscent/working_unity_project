using System.Collections;
using UnityEngine;

public class TurnActorAction : CutsceneAction
{
    [SerializeField] private CutsceneActor _actor;
    [SerializeField] private FacingDirection _direction;

    public override IEnumerator Play()
    {
        _actor.GetCharacter().Animator.SetFacingDirection(_direction);
        yield break;
    }
}
