using UnityEngine;

/// <summary>
/// Represents the current battlefield state.
/// </summary>
public class Field
{
    public Condition Weather { get; set; }
    public int? WeatherDuration { get; set; }

    /// <summary>
    /// Sets the current weather condition based on the provided condition ID.
    /// </summary>
    /// <param name="conditionID">The ID of the weather condition to apply.</param>
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