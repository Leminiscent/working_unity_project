using System.Collections.Generic;
using UnityEngine;

public static class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        // Non-volatile status conditions
        {
            ConditionID.Psn,
            new Condition
            {
                Name = "Poison",
                StartMessage = " has been poisoned!",
                FailMessage = " is already poisoned!",
                OnEndOfTurn = static battler =>
                {
                    battler.AddStatusEvent(StatusEventType.Damage, " is hurt by poison!", battler.MaxHp / 8);
                }
            }
        },
        {
            ConditionID.Brn,
            new Condition
            {
                Name = "Burn",
                StartMessage = " has been burned!",
                FailMessage = " is already burned!",
                OnEndOfTurn = static battler =>
                {
                    battler.AddStatusEvent(StatusEventType.Damage, " is hurt by its burn!", battler.MaxHp / 16);
                }
            }
        },
        {
            ConditionID.Slp,
            new Condition
            {
                Name = "Sleep",
                StartMessage = " has been put to sleep!",
                FailMessage = " is already asleep!",
                OnStartTimed = static battler => Random.Range(1, 4),
                OnBeginningOfTurnTimed = static (battler, timer) =>
                {
                    if (timer == 0)
                    {
                        battler.AddStatusEvent(StatusEventType.CureCondition, " woke up!");
                        return (true, 0);
                    }
                    timer--;
                    battler.AddStatusEvent(" is fast asleep!");
                    return (false, timer);
                }
            }
        },
        {
            ConditionID.Par,
            new Condition
            {
                Name = "Paralyzed",
                StartMessage = " has been paralyzed!",
                FailMessage = " is already paralyzed!",
                OnBeginningofTurn = static battler =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        battler.AddStatusEvent(" is fully paralyzed and cannot move!");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.Frz,
            new Condition
            {
                Name = "Frozen",
                StartMessage = " has been frozen solid!",
                FailMessage = " is already frozen!",
                OnBeginningofTurn = static battler =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        battler.CureStatus();
                        battler.AddStatusEvent(StatusEventType.CureCondition, " is no longer frozen!");
                        return true;
                    }
                    else
                    {
                        battler.AddStatusEvent(" is frozen solid and cannot move!");
                        return false;
                    }
                }
            }
        },

        // Volatile status conditions
        {
            ConditionID.Con,
            new Condition
            {
                Name = "Confusion",
                StartMessage = " has been confused!",
                FailMessage = " is already confused!",
                OnStartTimed = static battler => Random.Range(2, 5),
                OnBeginningOfTurnTimed = static (battler, timer) =>
                {
                    if (timer == 0)
                    {
                        battler.AddStatusEvent(StatusEventType.CureCondition, " snapped out of confusion!");
                        return (true, 0);
                    }
                    timer--;
                    if (Random.Range(1, 4) == 1)
                    {
                        battler.AddStatusEvent(StatusEventType.Damage, " hurt itself in its confusion!", battler.MaxHp / 8);
                        return (false, timer);
                    }
                    return (true, timer);
                }
            }
        },

        // Weather conditions
        {
            ConditionID.Sun,
            new Condition
            {
                Name = "Harsh Sunlight",
                StartMessage = "The sunlight turned harsh!",
                EffectMessage = "The sunlight is harsh!",
                OnDamageModify = static (source, target, move) =>
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
            ConditionID.Rain,
            new Condition
            {
                Name = "Heavy Rain",
                StartMessage = "It started to rain!",
                EffectMessage = "The rain is falling!",
                OnDamageModify = static (source, target, move) =>
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
            ConditionID.Sandstorm,
            new Condition
            {
                Name = "Sandstorm",
                StartMessage = "A sandstorm kicked up!",
                EffectMessage = "The sandstorm rages!",
                OnWeather = static battler =>
                {
                    battler.AddStatusEvent(StatusEventType.Damage, " is buffeted by the sandstorm!", battler.MaxHp / 16);
                }
            }
        },
        {
            ConditionID.Hail,
            new Condition
            {
                Name = "Hail",
                StartMessage = "It started to hail!",
                EffectMessage = "The hail is falling!",
                OnWeather = static battler =>
                {
                    battler.AddStatusEvent(StatusEventType.Damage, " is buffeted by the hail!", battler.MaxHp / 16);
                }
            }
        }
    };

    public static void Init()
    {
        foreach (KeyValuePair<ConditionID, Condition> kvp in Conditions)
        {
            Condition condition = kvp.Value;
            ConditionID conditionId = kvp.Key;

            // Assign the key as the condition's ID.
            condition.ID = conditionId;
        }
    }

    public static float GetStatusBonus(Dictionary<ConditionID, ConditionStatus> statuses)
    {
        if (statuses == null || statuses.Count == 0)
        {
            return 1f;
        }

        float bonus = 1f;
        foreach (KeyValuePair<ConditionID, ConditionStatus> entry in statuses)
        {
            Condition condition = entry.Value.Condition;
            if (condition.ID is ConditionID.Slp or ConditionID.Frz)
            {
                bonus += 1f;
            }
            else if (condition.ID is ConditionID.Psn or ConditionID.Brn or ConditionID.Par)
            {
                bonus += 0.5f;
            }
        }
        return bonus;
    }
}

public enum ConditionID
{
    None,
    Psn,
    Brn,
    Slp,
    Par,
    Frz,
    Con,
    Sun,
    Rain,
    Sandstorm,
    Hail
}