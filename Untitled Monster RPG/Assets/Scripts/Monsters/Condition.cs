using System;

public class Condition
{
    public ConditionID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }
    public string FailMessage { get; set; }
    public Action<Monster> OnStart { get; set; }
    public Func<Monster, bool> OnBeginningofTurn { get; set; }
    public Func<Monster, int> OnStartTimed { get; set; }
    public Func<Monster, int, (bool canAct, int newTimer)> OnBeginningOfTurnTimed { get; set; }
    public Action<Monster> OnEndOfTurn { get; set; }
    public Action<Monster> OnWeather { get; set; }
    public Func<Monster, Monster, Move, float> OnDamageModify { get; set; }
}

[Serializable]
public class ConditionSaveData
{
    public ConditionID ConditionId;
    public int Timer;
}