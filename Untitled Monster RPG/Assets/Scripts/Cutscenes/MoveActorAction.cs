using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveActorAction : CutsceneAction
{
    [SerializeField] Character actor;
    [SerializeField] List<Vector2> movePatterns;
}
