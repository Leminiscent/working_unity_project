using UnityEngine;

public class Field
{
    public Condition Weather { get; set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(ConditionID conditionID)
    {
        if (ConditionsDB.Conditions.TryGetValue(conditionID, out Condition condition))
        {
            Weather = condition;
            Weather.ID = conditionID;
            Weather.OnStart?.Invoke(null);
        }
        else
        {
            Debug.LogWarning($"Condition with ID {conditionID} was not found in the ConditionsDB.");
        }
    }
}