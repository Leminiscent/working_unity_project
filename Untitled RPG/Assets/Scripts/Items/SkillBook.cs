using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new skill book")]
public class SkillBook : ItemBase
{
    [SerializeField] private MoveBase _move;
    [SerializeField] private bool _isUnlimited;

    public override string Name => !_isUnlimited ? $"{_move.Name} Skill Book" : $"Ultimate {_move.Name} Skill Book";
    public override string Description => !_isUnlimited
                ? $"A book that when read by a battler, teaches it the move {_move.Name}. The book will be destroyed after a single use."
                : $"A book that when read by a battler, teaches it the move {_move.Name}.";
    public override bool IsReusable => _isUnlimited;
    public override bool UsableInBattle => false;
    public MoveBase Move => _move;
    public bool IsUnlimited => _isUnlimited;

    public override bool Use(Battler battler)
    {
        return battler.HasMove(_move);
    }

    public bool CanBeLearned(Battler battler)
    {
        return battler.Base.LearnableBySkillBook.Contains(_move);
    }
}