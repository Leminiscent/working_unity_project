using System.Collections.Generic;

public class BattleAction
{
    public BattleActionType ActionType { get; set; }
    public BattleUnit SourceUnit { get; set; }
    public List<BattleUnit> TargetUnits { get; set; }
    public Move SelectedMove { get; set; } // The selected move for a Fight action.
    public Battler SelectedBattler { get; set; } // The selected battler for a SwitchBattler action.
    public ItemBase SelectedItem { get; set; } // The selected item for a UseItem action.
    public bool IsValid { get; set; } = true;

    public int Priority => ActionType switch
    {
        BattleActionType.Fight => SelectedMove != null ? SelectedMove.Base.Priority : int.MaxValue,
        BattleActionType.UseItem => 95,
        BattleActionType.Guard => 96,
        BattleActionType.SwitchBattler => 97,
        BattleActionType.Talk => 98,
        BattleActionType.Run => 99,
        _ => int.MaxValue
    };
}

public enum BattleActionType
{
    Fight,
    Talk,
    UseItem,
    Guard,
    SwitchBattler,
    Run
}