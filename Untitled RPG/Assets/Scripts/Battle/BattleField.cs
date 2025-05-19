using UnityEngine;

public class BattleField
{
    public WeatherCondition Weather { get; set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(WeatherConditionID conditionID)
    {
        if (WeatherConditionDB.Conditions.TryGetValue(conditionID, out WeatherCondition condition))
        {
            Weather = condition;
            Weather.ID = conditionID;
        }
        else
        {
            Debug.LogWarning($"Weather condition with ID {conditionID} was not found in the WeatherConditionsDB.");
        }
    }
}