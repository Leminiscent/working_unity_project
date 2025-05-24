using UnityEngine;

public class BattleField
{
    public WeatherCondition DefaultWeather { get; set; } = null;
    public WeatherCondition Weather { get; set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(WeatherConditionID conditionID, int? duration = null)
    {
        if (WeatherConditionDB.Conditions.TryGetValue(conditionID, out WeatherCondition condition))
        {
            Weather = condition;
            WeatherDuration = duration;
        }
        else
        {
            Debug.LogWarning($"Weather condition with ID {conditionID} was not found in the WeatherConditionsDB.");
        }
    }
}