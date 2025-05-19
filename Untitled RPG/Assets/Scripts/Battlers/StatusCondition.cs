using System;

public class StatusCondition
{
    public StatusConditionID ID { get; set; }
    public string Name { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }
    public string FailMessage { get; set; }
    public Func<Battler, bool> OnBeginningofTurn { get; set; }
    public Func<Battler, int> OnStartTimed { get; set; }
    public Func<Battler, int, (bool canAct, int newTimer)> OnBeginningOfTurnTimed { get; set; }
    public Action<Battler> OnEndOfTurn { get; set; }
}

public class ConditionStatus
{
    public StatusCondition Condition { get; private set; }
    public int Timer { get; set; }

    public ConditionStatus(StatusCondition condition, int timer)
    {
        Condition = condition;
        Timer = timer;
    }
}

[Serializable]
public class ConditionSaveData
{
    public StatusConditionID ConditionId;
    public int Timer;
}