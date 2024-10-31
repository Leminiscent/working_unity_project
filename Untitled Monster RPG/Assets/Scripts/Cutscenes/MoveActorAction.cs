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
        Character character = _actor.GetCharacter();

        foreach (Vector2 movePattern in _movePatterns)
        {
            yield return character.Move(movePattern, checkCollisions: false);
        }
    }
}

[System.Serializable]
public class CutsceneActor
{
    [SerializeField] private bool _isPlayer;
    [SerializeField] private Character _character;

    public Character GetCharacter() => _isPlayer ? PlayerController.Instance.Character : _character;
}