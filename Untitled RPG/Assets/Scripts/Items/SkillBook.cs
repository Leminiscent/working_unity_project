using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Items/Create new skill book")]
public class SkillBook : ItemBase
{
    [field: SerializeField, FormerlySerializedAs("_move")] public MoveBase Move { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_isUnlimited")] public bool IsUnlimited { get; private set; }

    public override string Name => !IsUnlimited
        ? $"{Move.Name} Skill Book"
        : $"Ultimate {Move.Name} Skill Book";

    public override string Description => !IsUnlimited
        ? $"A book that when read by a battler, teaches it the move {Move.Name}. The book will be destroyed after a single use."
        : $"A book that when read by a battler, teaches it the move {Move.Name}.";

    public override bool IsReusable => IsUnlimited;
    public override bool UsableInBattle => false;

    public override bool Use(Battler battler)
    {
        // Returns whether the battler already has the move.
        return battler.HasMove(Move);
    }

    public bool CanBeLearned(Battler battler)
    {
        return battler.Base.LearnableBySkillBook.Contains(Move);
    }
}