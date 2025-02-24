using System.Collections.Generic;

public class BattleAction
{
    public BattleActionType ActionType { get; set; }
    public BattleUnit SourceUnit { get; set; }
    public List<BattleUnit> TargetUnits { get; set; }
    public Move SelectedMove { get; set; }
    public Monster SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool IsValid { get; set; } = true;
    public int Priority => (ActionType == BattleActionType.Fight) ? SelectedMove.Base.Priority
    : (ActionType == BattleActionType.UseItem) ? 95
    : (ActionType == BattleActionType.Guard) ? 96
    : (ActionType == BattleActionType.SwitchMonster) ? 97
    : (ActionType == BattleActionType.Talk) ? 98
    : 99;
}

public enum BattleActionType
{
    Fight,
    Talk,
    UseItem,
    Guard,
    SwitchMonster,
    Run
}