using System.Collections;
using UnityEngine;

public class TurnActorAction : CutsceneAction
{
    [SerializeField] private CutsceneActor _actor;
    [SerializeField] private FacingDirection _direction;

    public override IEnumerator Play()
    {
        if (_actor == null)
        {
            Debug.LogWarning("Actor is not assigned in TurnActorAction.");
            yield break;
        }
        
        _actor.GetCharacter().Animator.SetFacingDirection(_direction);
        yield break;
    }
}