using System.Collections.Generic;
using UnityEngine;

public static class StatusConditionDB
{
    public static Dictionary<StatusConditionID, StatusCondition> Conditions { get; set; } = new Dictionary<StatusConditionID, StatusCondition>()
    {
        // Non-volatile status conditions
        {
            StatusConditionID.Psn,
            new StatusCondition
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
            StatusConditionID.Brn,
            new StatusCondition
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
            StatusConditionID.Slp,
            new StatusCondition
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
            StatusConditionID.Par,
            new StatusCondition
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
            StatusConditionID.Frz,
            new StatusCondition
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
            StatusConditionID.Con,
            new StatusCondition
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
    };

    public static void Init()
    {
        foreach (KeyValuePair<StatusConditionID, StatusCondition> kvp in Conditions)
        {
            StatusCondition condition = kvp.Value;
            StatusConditionID conditionId = kvp.Key;

            // Assign the key as the condition's ID.
            condition.ID = conditionId;
        }
    }

    public static float GetStatusBonus(Dictionary<StatusConditionID, ConditionStatus> statuses)
    {
        if (statuses == null || statuses.Count == 0)
        {
            return 1f;
        }

        float bonus = 1f;
        foreach (KeyValuePair<StatusConditionID, ConditionStatus> entry in statuses)
        {
            StatusCondition condition = entry.Value.Condition;
            if (condition.ID is StatusConditionID.Slp or StatusConditionID.Frz)
            {
                bonus += 1f;
            }
            else if (condition.ID is StatusConditionID.Psn or StatusConditionID.Brn or StatusConditionID.Par)
            {
                bonus += 0.5f;
            }
        }
        return bonus;
    }
}

public enum StatusConditionID
{
    None,
    Psn,
    Brn,
    Slp,
    Par,
    Frz,
    Con,
}