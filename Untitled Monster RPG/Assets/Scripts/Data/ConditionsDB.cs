using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ConditionsDB : MonoBehaviour
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
        {
            ConditionID.psn,
            new Condition
            {
                Name = "Poison",
                StartMessage = "has been poisoned!",
                OnEndOfTurn = (Monster monster) =>
                {
                    monster.UpdateHP(monster.MaxHp / 8);
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
                    monster.UpdateHP(monster.MaxHp / 16);
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
                        monster.UpdateHP(Mathf.FloorToInt(monster.MaxHp / 8));
                        monster.StatusChanges.Enqueue($"{monster.Base.Name} hurt itself in its confusion!");
                        return false;
                    }
                    return true;
                }
            }
        }
    };
}

public enum ConditionID
{
    none,
    psn,
    brn,
    slp,
    par,
    frz,
    confusion
}