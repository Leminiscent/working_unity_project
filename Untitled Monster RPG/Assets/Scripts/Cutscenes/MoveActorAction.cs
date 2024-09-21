using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveActorAction : CutsceneAction
{
    [SerializeField] CutsceneActor actor;
    [SerializeField] List<Vector2> movePatterns;

    public override IEnumerator Play()
    {
        var character = actor.GetCharacter();

        foreach (var movePattern in movePatterns)
        {
            yield return character.Move(movePattern, checkCollisions: false);
        }
    }
}

[System.Serializable]
public class CutsceneActor
{
    [SerializeField] bool isPlayer;
    [SerializeField] Character character;

    public Character GetCharacter() => isPlayer ? PlayerController.Instance.Character : character;
}