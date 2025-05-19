using System;
using System.Collections.Generic;

public class WeatherConditionDB
{
    public static Dictionary<WeatherConditionID, WeatherCondition> Conditions { get; set; } = new Dictionary<WeatherConditionID, WeatherCondition>()
    {
        {
            WeatherConditionID.Sun,
            new WeatherCondition
            {
                Name = "Harsh Sunlight",
                StartMessage = "The sunlight turned harsh!",
                EffectMessage = "The sunlight is harsh!",
                EndMessage = "The sunlight faded!",
                OnDamageModify = static (move) =>
                {
                    if (move.Base.Type == BattlerType.Fire)
                    {
                        return 1.5f;
                    }
                    else if (move.Base.Type == BattlerType.Water)
                    {
                        return 0.5f;
                    }
                    return 1f;
                }
            }
        },
        {
            WeatherConditionID.Rain,
            new WeatherCondition
            {
                Name = "Heavy Rain",
                StartMessage = "It started to rain!",
                EffectMessage = "The rain is falling!",
                EndMessage = "The rain stopped!",
                OnDamageModify = static (move) =>
                {
                    if (move.Base.Type == BattlerType.Water)
                    {
                        return 1.5f;
                    }
                    else if (move.Base.Type == BattlerType.Fire)
                    {
                        return 0.5f;
                    }
                    return 1f;
                }
            }
        },
        {
            WeatherConditionID.Sandstorm,
            new WeatherCondition
            {
                Name = "Sandstorm",
                StartMessage = "A sandstorm kicked up!",
                EffectMessage = "The sandstorm rages!",
                EndMessage = "The sandstorm subsided!",
                OnWeatherEffect = static battler =>
                {
                    if (!battler.HasType(BattlerType.Earth))
                    {
                        battler.AddStatusEvent(StatusEventType.Damage, " is buffeted by the sandstorm!", battler.MaxHp / 16);
                    }
                }
            }
        },
        {
            WeatherConditionID.Hail,
            new WeatherCondition
            {
                Name = "Hail",
                StartMessage = "It started to hail!",
                EffectMessage = "The hail is falling!",
                EndMessage = "The hail stopped!",
                OnWeatherEffect = static battler =>
                {
                    if (!battler.HasType(BattlerType.Ice))
                    {
                        battler.AddStatusEvent(StatusEventType.Damage, " is buffeted by the hail!", battler.MaxHp / 16);
                    }
                }
            }
        }
    };

    public static void Init()
    {
        foreach (KeyValuePair<WeatherConditionID, WeatherCondition> kvp in Conditions)
        {
            WeatherCondition condition = kvp.Value;
            WeatherConditionID conditionId = kvp.Key;

            // Assign the key as the condition's ID.
            condition.ID = conditionId;
        }
    }
}

public class WeatherCondition
{
    public WeatherConditionID ID { get; set; }
    public string Name { get; set; }
    public string StartMessage { get; set; }
    public string EffectMessage { get; set; }
    public string EndMessage { get; set; }

    public Action<Battler> OnWeatherEffect { get; set; }
    public Func<Move, float> OnDamageModify { get; set; }
}

public enum WeatherConditionID
{
    None,
    Sun,
    Rain,
    Sandstorm,
    Hail,
}