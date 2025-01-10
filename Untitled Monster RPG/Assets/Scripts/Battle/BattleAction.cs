public class BattleAction
{
    public BattleActionType ActionType { get; set; }
    public BattleUnit SourceUnit { get; set; }
    public BattleUnit TargetUnit { get; set; }
    public Move SelectedMove { get; set; }
    public Monster SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }
    public int Priority => (ActionType == BattleActionType.Fight) ? SelectedMove.Base.Priority : 99;
}

public enum BattleActionType
{
    Fight,
    Talk,
    UseItem,
    SwitchMonster,
    Run
}