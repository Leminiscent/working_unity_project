using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field
{
    public Condition Weather { get; set; }
    public int? WeatherDuration { get; set; }

    public void SetWeather(ConditionID conditionID)
    {
        Weather = ConditionsDB.Conditions[conditionID];
        Weather.ID = conditionID;
        Weather.OnStart?.Invoke(null);
    }
}
