using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new skill book")]
public class SkillBook : ItemBase
{
    [SerializeField] MoveBase move;

    public MoveBase Move => move;

    public override bool Use(Monster monster)
    {
        return monster.HasMove(move);
    }
}