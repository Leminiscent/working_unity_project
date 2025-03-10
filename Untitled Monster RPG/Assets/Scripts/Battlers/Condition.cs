using System;

public class Condition
{
    public ConditionID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }
    public string FailMessage { get; set; }
    public Action<Battler> OnStart { get; set; }
    public Func<Battler, bool> OnBeginningofTurn { get; set; }
    public Func<Battler, int> OnStartTimed { get; set; }
    public Func<Battler, int, (bool canAct, int newTimer)> OnBeginningOfTurnTimed { get; set; }
    public Action<Battler> OnEndOfTurn { get; set; }
    public Action<Battler> OnWeather { get; set; }
    public Func<Battler, Battler, Move, float> OnDamageModify { get; set; }
}

[Serializable]
public class ConditionSaveData
{
    public ConditionID ConditionId;
    public int Timer;
}