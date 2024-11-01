using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new skill book")]
public class SkillBook : ItemBase
{
    [SerializeField] private MoveBase move;
    [SerializeField] private bool isUnlimited;

    public override string Name
    {
        get
        {
            return !isUnlimited ? $"{move.Name} Skill Book" : $"Ultimate {move.Name} Skill Book";
        }
    }
    public override string Description
    {
        get
        {
            return !isUnlimited
                ? $"A book that when read by a monster, teaches it the move {move.Name}. The book will be destroyed after a single use."
                : $"A book that when read by a monster, teaches it the move {move.Name}.";
        }
    }

    public override bool Use(Monster monster)
    {
        return monster.HasMove(move);
    }

    public bool CanBeLearned(Monster monster)
    {
        return monster.Base.LearnableBySkillBook.Contains(move);
    }

    public override bool IsReusable => isUnlimited;
    public override bool UsableInBattle => false;

    public MoveBase Move => move;
    public bool IsUnlimited => isUnlimited;
}