using System;

public class Condition
{
    public ConditionID ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; }
    public Action<Monster> OnStart { get; set; }
    public Func<Monster, bool> OnBeginningofTurn { get; set; }
    public Action<Monster> OnEndOfTurn { get; set; }
}
