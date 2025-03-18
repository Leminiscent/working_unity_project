using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveActorAction : CutsceneAction
{
    [SerializeField] private CutsceneActor _actor;
    [SerializeField] private List<Vector2> _movePatterns;

    public override IEnumerator Play()
    {
        if (_actor == null)
        {
            Debug.LogWarning("Actor is not assigned in MoveActorAction.");
            yield break;
        }

        Character character = _actor.GetCharacter();

        if (character == null)
        {
            Debug.LogWarning("Character is not assigned in MoveActorAction.");
            yield break;
        }

        if (_movePatterns == null || _movePatterns.Count == 0)
        {
            Debug.LogWarning("Move patterns are not assigned in MoveActorAction.");
            yield break;
        }

        foreach (Vector2 movePattern in _movePatterns)
        {
            yield return character.MoveRoutine(movePattern, checkCollisions: false);
        }
    }
}

[System.Serializable]
public class CutsceneActor
{
    [SerializeField] private bool _isPlayer;
    [SerializeField] private Character _character;

    public Character GetCharacter()
    {
        return _isPlayer ? PlayerController.Instance.Character : _character;
    }
}