using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Battler
{
    [SerializeField] private BattlerBase _base;
    [SerializeField] private int _level;

    public BattlerBase Base => _base;
    public int Level => _level;
    public bool IsCommander { get; set; }
    public bool HasJustLeveledUp { get; set; }
    public int Exp { get; set; }
    public int Hp { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public bool IsGuarding { get; set; }
    public Dictionary<ConditionID, (Condition, int)> Statuses { get; private set; }
    public Dictionary<ConditionID, (Condition, int)> VolatileStatuses { get; private set; }
    public Queue<StatusEvent> StatusChanges { get; private set; }
    public int AffinityLevel { get; set; }
    public event Action OnStatusChanged;
    public event Action OnHPChanged;
    public int MaxHp { get; private set; }
    public int Strength => GetStat(Stat.Strength);
    public int Endurance => GetStat(Stat.Endurance);
    public int Intelligence => GetStat(Stat.Intelligence);
    public int Fortitude => GetStat(Stat.Fortitude);
    public int Agility => GetStat(Stat.Agility);
    public Dictionary<Stat, float> StatPerformanceValues { get; private set; }

    public Battler(BattlerBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;

        Init();
    }

    public void Init()
    {
        Moves = new List<Move>();
        for (int i = Base.LearnableMoves.Count - 1; i >= 0; i--)
        {
            LearnableMove move = Base.LearnableMoves[i];
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= BattlerBase.MaxMoveCount)
            {
                break;
            }
        }

        StatPerformanceValues = new Dictionary<Stat, float>()
        {
            { Stat.HP, 0 },
            { Stat.Strength, 0 },
            { Stat.Endurance, 0 },
            { Stat.Intelligence, 0 },
            { Stat.Fortitude, 0 },
            { Stat.Agility, 0 }
        };

        Exp = Base.GetExpForLevel(Level);
        CalculateStats();
        Hp = MaxHp;

        StatusChanges = new Queue<StatusEvent>();
        ResetStatBoosts();
        Statuses = new Dictionary<ConditionID, (Condition, int)>();
        VolatileStatuses = new Dictionary<ConditionID, (Condition, int)>();
        AffinityLevel = 0;
    }

    public Battler(BattlerSaveData saveData)
    {
        _base = BattlerDB.GetObjectByName(saveData.Name);
        Hp = saveData.Hp;
        _level = saveData.Level;
        Exp = saveData.Exp;

        Statuses = new Dictionary<ConditionID, (Condition, int)>();

        if (saveData.Statuses != null && saveData.Statuses.Count > 0)
        {
            foreach (ConditionSaveData statusSave in saveData.Statuses)
            {
                Condition condition = ConditionsDB.Conditions[statusSave.ConditionId];
                Statuses.Add(statusSave.ConditionId, (condition, statusSave.Timer));
            }
        }

        Moves = saveData.Moves.Select(static s => new Move(s)).ToList();
        StatPerformanceValues = saveData.StatPerformanceValues.ToDictionary(static s => s.Stat, static s => s.Pv);

        CalculateStats();
        StatusChanges = new Queue<StatusEvent>();
        ResetStatBoosts();
    }

    public BattlerSaveData GetSaveData()
    {
        BattlerSaveData saveData = new()
        {
            Name = Base.Name,
            Hp = Hp,
            Level = Level,
            Exp = Exp,
            Statuses = Statuses.Select(static s => new ConditionSaveData
            {
                ConditionId = s.Key,
                Timer = s.Value.Item2
            }).ToList(),
            Moves = Moves.Select(static m => m.GetSaveData()).ToList(),
            StatPerformanceValues = StatPerformanceValues.Select(static s => new StatPV
            {
                Stat = s.Key,
                Pv = s.Value
            }).ToList()
        };

        return saveData;
    }

    private void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Strength, Mathf.FloorToInt((((2f * Base.Strength) + (StatPerformanceValues[Stat.Strength] / 4f)) * Level / 100f) + 5f) },
            { Stat.Endurance, Mathf.FloorToInt((((2f * Base.Endurance) + (StatPerformanceValues[Stat.Endurance] / 4f)) * Level / 100f) + 5f) },
            { Stat.Intelligence, Mathf.FloorToInt((((2f * Base.Intelligence) + (StatPerformanceValues[Stat.Intelligence] / 4f)) * Level / 100f) + 5f) },
            { Stat.Fortitude, Mathf.FloorToInt((((2f * Base.Fortitude) + (StatPerformanceValues[Stat.Fortitude] / 4f)) * Level / 100f) + 5f) },
            { Stat.Agility, Mathf.FloorToInt((((2f * Base.Agility) + (StatPerformanceValues[Stat.Agility] / 4f)) * Level / 100f) + 5f) }
        };

        int prevMaxHp = MaxHp;
        MaxHp = Mathf.FloorToInt((((2f * Base.HP) + (StatPerformanceValues[Stat.HP] / 4f)) * Level / 100f) + Level + 10f);
        Hp += MaxHp - prevMaxHp;
        if (MaxHp > prevMaxHp)
        {
            OnHPChanged?.Invoke();
        }
    }

    public void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Strength, 0 },
            { Stat.Endurance, 0 },
            { Stat.Intelligence, 0 },
            { Stat.Fortitude, 0 },
            { Stat.Agility, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 }
        };
    }

    private int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        int boost = StatBoosts[stat];
        float[] boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        statVal = boost >= 0 ? Mathf.FloorToInt(statVal * boostValues[boost]) : Mathf.FloorToInt(statVal / boostValues[-boost]);

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (StatBoost statBoost in statBoosts)
        {
            Stat stat = statBoost.Stat;
            int boost = statBoost.Boost;
            bool changeIsPositive = boost > 0;
            string riseOrFall;

            if ((changeIsPositive && StatBoosts[stat] == 6) || (!changeIsPositive && StatBoosts[stat] == -6))
            {
                riseOrFall = changeIsPositive ? "higher" : "lower";
                AddStatusEvent($"'s {stat} won't go any {riseOrFall}!");
                return;
            }

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            riseOrFall = changeIsPositive ? "rose" : "fell";

            string bigChange = (Mathf.Abs(boost) >= 3) ? " severly " : (Mathf.Abs(boost) == 2) ? " harshly " : " ";

            AddStatusEvent(StatusEventType.StatBoost, $"{(Base.Name.EndsWith("s") ? "'" : "'s")} {stat}{bigChange}{riseOrFall}!", boost);
        }
    }

    public void GainPvs(Dictionary<Stat, float> pvGained)
    {
        foreach (KeyValuePair<Stat, float> spv in StatPerformanceValues.ToArray())
        {
            if (spv.Value < GlobalSettings.Instance.MaxPvPerStat && GetTotalPvs() < GlobalSettings.Instance.MaxPvs)
            {
                pvGained[spv.Key] = Mathf.Clamp(pvGained[spv.Key], 0, GlobalSettings.Instance.MaxPvs - GetTotalPvs());
                StatPerformanceValues[spv.Key] = Mathf.Clamp(StatPerformanceValues[spv.Key] += pvGained[spv.Key], 0, GlobalSettings.Instance.MaxPvPerStat);
            }
        }
    }

    public float GetTotalPvs()
    {
        return StatPerformanceValues.Values.Sum();
    }

    public bool CheckForLevelUp()
    {
        if (Level >= GlobalSettings.Instance.MaxLevel)
        {
            return false;
        }

        if (Exp >= Base.GetExpForLevel(Level + 1))
        {
            ++_level;
            CalculateStats();
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.FirstOrDefault(x => x.Level == Level);
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > BattlerBase.MaxMoveCount)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn));
    }

    public bool HasMove(MoveBase move)
    {
        return Moves.Count(m => m.Base == move) > 0;
    }

    public bool HasType(BattlerType type)
    {
        return (_base.Type1 == type) || (_base.Type2 == type);
    }

    public Transformation CheckForTransformation()
    {
        return Base.Transformations.FirstOrDefault(t => t.RequiredLevel <= Level);
    }

    public Transformation CheckForTransformation(ItemBase item)
    {
        return Base.Transformations.FirstOrDefault(t => t.RequiredItem == item);
    }

    public void Transform(Transformation transformation)
    {
        _base = transformation.TransformsInto;
        CalculateStats();
    }

    public void RestoreBattler()
    {
        Hp = MaxHp;
        OnHPChanged?.Invoke();
        Statuses.Clear();
        OnStatusChanged?.Invoke();
        ResetStatBoosts();
        foreach (Move move in Moves)
        {
            move.Sp = move.Base.SP;
        }
    }

    public float GetNormalizedExp()
    {
        int currLevelExp = Base.GetExpForLevel(Level);
        int nextLevelExp = Base.GetExpForLevel(Level + 1);
        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);

        return Mathf.Clamp01(normalizedExp);
    }

    public DamageDetails TakeDamage(Move move, Battler attacker, Condition weather)
    {
        if (move.Base.OneHitKO.IsOneHitKO)
        {
            int oneHitDamage = Hp;

            DecreaseHP(oneHitDamage);
            return new DamageDetails() { TypeEffectiveness = 1f, Critical = 1f, Defeated = false };
        }

        float critical = 1f;

        if (!(move.Base.CritBehavior == CritBehavior.NeverCrits))
        {
            if (move.Base.CritBehavior == CritBehavior.AlwaysCrits)
            {
                critical = 1.5f;
            }
            else
            {
                int critChance = 0 + ((move.Base.CritBehavior == CritBehavior.HighCritRatio) ? 1 : 0);
                float[] chances = new float[] { 4.167f, 12.5f, 50f, 100f };

                if (UnityEngine.Random.value * 100f <= chances[Mathf.Clamp(critChance, 0, 3)])
                {
                    critical = 1.5f;
                }
            }
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);
        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;
        DamageDetails damageDetails = new()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Defeated = false,
            TotalDamageDealt = 0,
            ActualDamageDealt = 0
        };
        float attack = (move.Base.Category == MoveCategory.Magical) ? attacker.Intelligence : attacker.Strength;
        float defense = (move.Base.Category == MoveCategory.Magical) ? Fortitude : Endurance;
        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical * weatherMod;
        float a = ((2 * attacker.Level) + 10) / 250f;
        float d = a * move.Base.Power * (((float)attack / defense) + 2);
        int damage = !IsGuarding ? Mathf.FloorToInt(d * modifiers) : Mathf.FloorToInt(d * modifiers / 2f);

        damageDetails.TotalDamageDealt = damage;
        damageDetails.ActualDamageDealt = Mathf.Min(damage, Hp);
        DecreaseHP(damage);
        return damageDetails;
    }

    public void TakeRecoilDamage(int damage)
    {
        if (damage < 1)
        {
            damage = 1;
        }
        AddStatusEvent(StatusEventType.Damage, $" was damaged by the recoil!", damage);
    }

    public void DrainHealth(int heal, string targetName)
    {
        if (heal < 1)
        {
            heal = 1;
        }
        AddStatusEvent(StatusEventType.Heal, $" drained health from {targetName}!", heal);
    }

    public void DecreaseHP(int damage)
    {
        Hp = Mathf.Clamp(Hp - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int heal)
    {
        Hp = Mathf.Clamp(Hp + heal, 0, MaxHp);
        OnHPChanged?.Invoke();
        AudioManager.Instance.PlaySFX(AudioID.Heal);
    }

    public void UpdateAffinityLevel(int affinity)
    {
        AffinityLevel = Mathf.Clamp(AffinityLevel + affinity, 0, 6);
    }

    public void AddCondition(ConditionID conditionId, bool isVolatile = false)
    {
        if (conditionId == ConditionID.None)
        {
            return;
        }

        Dictionary<ConditionID, (Condition, int)> statuses = isVolatile ? VolatileStatuses : Statuses;

        if (statuses.ContainsKey(conditionId))
        {
            return;
        }

        Condition condition = ConditionsDB.Conditions[conditionId];
        int timer = 0;
        if (condition.OnStartTimed != null)
        {
            timer = condition.OnStartTimed(this);
        }

        statuses.Add(conditionId, (condition, timer));
        if (!string.IsNullOrEmpty(condition.StartMessage))
        {
            AddStatusEvent(StatusEventType.SetCondition, condition.StartMessage);
        }

        OnStatusChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId)
    {
        AddCondition(conditionId);
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        AddCondition(conditionId, true);
    }

    public void RemoveCondition(ConditionID conditionId)
    {
        if (Statuses.ContainsKey(conditionId))
        {
            Statuses.Remove(conditionId);
            OnStatusChanged?.Invoke();
            AudioManager.Instance.PlaySFX(AudioID.CureStatus);
        }
    }

    public void CureStatus()
    {
        Statuses.Clear();
        OnStatusChanged?.Invoke();
        AudioManager.Instance.PlaySFX(AudioID.CureStatus);
    }

    public void CureVolatileStatus()
    {
        CureStatus();
    }

    public void CureAllStatus()
    {
        CureStatus();
    }

    public Move GetRandomMove()
    {
        List<Move> movesWithSP = Moves.Where(static x => x.Sp > 0).ToList();

        if (movesWithSP.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, movesWithSP.Count);

        return movesWithSP[randomIndex];
    }

    public bool OnStartOfTurn()
    {
        bool canPerformMove = true;
        foreach (ConditionID key in Statuses.Keys.ToList())
        {
            (Condition condition, int timer) = Statuses[key];
            if (condition.OnBeginningOfTurnTimed != null)
            {
                (bool canAct, int newTimer) = condition.OnBeginningOfTurnTimed(this, timer);
                if (newTimer <= 0)
                {
                    RemoveCondition(key);
                }
                else
                {
                    Statuses[key] = (condition, newTimer);
                }
                if (!canAct)
                {
                    canPerformMove = false;
                }
            }
            else if (condition.OnBeginningofTurn != null)
            {
                if (!condition.OnBeginningofTurn(this))
                {
                    canPerformMove = false;
                }
            }
        }
        return canPerformMove;
    }

    public void OnEndOfTurn()
    {
        foreach (KeyValuePair<ConditionID, (Condition, int)> kvp in Statuses.ToList())
        {
            Condition condition = kvp.Value.Item1;
            condition.OnEndOfTurn?.Invoke(this);
        }
    }

    public void OnBattleOver()
    {
        VolatileStatuses.Clear();
        ResetStatBoosts();
        CalculateStats();
    }

    public void AddStatusEvent(StatusEventType type, string message, int? value = null)
    {
        StatusChanges.Enqueue(new StatusEvent(type, message, value));
    }

    public void AddStatusEvent(string message)
    {
        AddStatusEvent(StatusEventType.Text, message, null);
    }
}

[Serializable]
public class BattlerSaveData
{
    public string Name;
    public int Hp;
    public int Level;
    public int Exp;
    public List<ConditionSaveData> Statuses;
    public List<MoveSaveData> Moves;
    public List<StatPV> StatPerformanceValues;
}

[Serializable]
public class StatPV
{
    public Stat Stat;
    public float Pv;
}

public class DamageDetails
{
    public bool Defeated { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
    public int TotalDamageDealt { get; set; }
    public int ActualDamageDealt { get; set; }
}

public class StatusEvent
{
    public StatusEventType Type { get; private set; }
    public string Message { get; private set; }
    public int? Value { get; private set; }

    public StatusEvent(StatusEventType type, string message, int? value)
    {
        Type = type;
        Value = value;
        Message = message;
    }
}

public enum StatusEventType
{
    Text,
    Damage,
    Heal,
    SetCondition,
    CureCondition,
    StatBoost
}