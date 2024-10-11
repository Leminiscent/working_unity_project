using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var condition = kvp.Value;
            var conditionId = kvp.Key;

            condition.ID = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        // Non-volatile status conditions
        {
            ConditionID.psn,
            new Condition
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnEndOfTurn = (Monster monster) =>
                {
                    monster.DecreaseHP(monster.MaxHp / 8);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is hurt by poison!");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition
            {
                Name = "Burn",
                StartMessage = "has been burned!",
                OnEndOfTurn = (Monster monster) =>
                {
                    monster.DecreaseHP(monster.MaxHp / 16);
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is hurt by its burn!");
                }
            }
        },
        {
            ConditionID.slp,
            new Condition
            {
                Name = "Sleep",
                StartMessage = "has been put to sleep!",
                OnStart = (Monster monster) =>
                {
                    monster.StatusTime = Random.Range(1, 4);
                },
                OnBeginningofTurn = (Monster monster) =>
                {
                    if (monster.StatusTime == 0)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} woke up!");
                        return true;
                    }
                    monster.StatusTime--;
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is fast asleep!");
                    return false;
                }
            }
        },
        {
            ConditionID.par,
            new Condition
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed!",
                OnBeginningofTurn = (Monster monster) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is fully paralyzed and cannot move!");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition
            {
                Name = "Frozen",
                StartMessage = "has been frozen solid!",
                OnBeginningofTurn = (Monster monster) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        monster.CureStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is no longer frozen!");
                        return true;
                    }
                    else
                    {
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is frozen solid and cannot move!");
                        return false;
                    }
                }
            }
        },

        // Volatile status conditions
        {
            ConditionID.confusion,
            new Condition
            {
                Name = "Confusion",
                StartMessage = "has been confused!",
                OnStart = (Monster monster) =>
                {
                    monster.VolatileStatusTime = Random.Range(2, 5);
                },
                OnBeginningofTurn = (Monster monster) =>
                {
                    if (monster.VolatileStatusTime == 0)
                    {
                        monster.CureVolatileStatus();
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} snapped out of confusion!");
                        return true;
                    }
                    monster.VolatileStatusTime--;

                    if (Random.Range(1, 4) == 1)
                    {
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} is confused!");
                        monster.DecreaseHP(Mathf.FloorToInt(monster.MaxHp / 8));
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself in its confusion!");
                        return false;
                    }
                    return true;
                }
            }
        },

        // Weather conditions
        {
            ConditionID.sunny,
            new Condition()
            {
                Name = "Harsh Sunlight",
                StartMessage = "The sunlight turned harsh!",
                EffectMessage = "The sunlight is harsh!",
                OnDamageModify = (Monster source, Monster target, Move move) =>
                {
                    if (move.Base.Type == MonsterType.Fire)
                    {
                        return 1.5f;
                    }
                    else if (move.Base.Type == MonsterType.Water)
                    {
                        return 0.5f;
                    }

                    return 1f;
                }
            }
        },
        {
            ConditionID.rain,
            new Condition()
            {
                Name = "Heavy Rain",
                StartMessage = "It started to rain!",
                EffectMessage = "The rain is falling!",
                OnDamageModify = (Monster source, Monster target, Move move) =>
                {
                    if (move.Base.Type == MonsterType.Water)
                    {
                        return 1.5f;
                    }
                    else if (move.Base.Type == MonsterType.Fire)
                    {
                        return 0.5f;
                    }

                    return 1f;
                }
            }
        },
        {
            ConditionID.sandstorm,
            new Condition()
            {
                Name = "Sandstorm",
                StartMessage = "A sandstorm kicked up!",
                EffectMessage = "The sandstorm rages!",
                OnWeather = (Monster monster) =>
                {
                    monster.DecreaseHP(Mathf.RoundToInt((float)monster.MaxHp / 16f));
                    monster.StatusChanges.Enqueue($"{monster.Base.Name} is buffeted by the sandstorm!");
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
        {
            return 1f;
        }
        else if (condition.ID == ConditionID.slp || condition.ID == ConditionID.frz)
        {
            return 2f;
        }
        else if (condition.ID == ConditionID.psn || condition.ID == ConditionID.brn || condition.ID == ConditionID.par)
        {
            return 1.5f;
        }
        else
        {
            return 1f;
        }
    }
}

public enum ConditionID
{
    none,
    psn,
    brn,
    slp,
    par,
    frz,
    confusion,
    sunny,
    rain,
    sandstorm,
}