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
    public Dictionary<ConditionID, ConditionStatus> Statuses { get; private set; }
    public Dictionary<ConditionID, ConditionStatus> VolatileStatuses { get; private set; }
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

    // Constants for stat formulas and boost limits
    private const float NON_HP_BASE_ADD = 5f;
    private const float HP_LEVEL_ADD = 10f;
    private const int MAX_STAT_BOOST = 6;
    private const int MIN_STAT_BOOST = -6;

    public Battler(BattlerBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;
        InitBattler();
    }

    public Battler(BattlerSaveData saveData)
    {
        _base = BattlerDB.GetObjectByName(saveData.Name);
        Hp = saveData.Hp;
        _level = saveData.Level;
        Exp = saveData.Exp;
        Moves = saveData.Moves.Select(static s => new Move(s)).ToList();
        StatPerformanceValues = saveData.StatPerformanceValues.ToDictionary(static s => s.Stat, static s => s.Pv);
        InitCommon();

        // Load saved statuses if any
        Statuses = new Dictionary<ConditionID, ConditionStatus>();
        VolatileStatuses = new Dictionary<ConditionID, ConditionStatus>();
        if (saveData.Statuses != null && saveData.Statuses.Count > 0)
        {
            foreach (ConditionSaveData statusSave in saveData.Statuses)
            {
                Condition condition = ConditionsDB.Conditions[statusSave.ConditionId];
                Statuses.Add(statusSave.ConditionId, new ConditionStatus(condition, statusSave.Timer));
            }
        }
        CalculateStats();
    }

    public void InitBattler()
    {
        InitMoves();
        InitCommon();
        Exp = Base.GetExpForLevel(Level);
        CalculateStats();
        Hp = MaxHp;
    }

    private void InitCommon()
    {
        Moves ??= new List<Move>();
        StatusChanges = new Queue<StatusEvent>();
        ResetStatBoosts();
        Statuses = new Dictionary<ConditionID, ConditionStatus>();
        VolatileStatuses = new Dictionary<ConditionID, ConditionStatus>();
        AffinityLevel = 0;

        // Initialize performance values for all stats with 0
        StatPerformanceValues = new Dictionary<Stat, float>
        {
            { Stat.HP, 0 },
            { Stat.Strength, 0 },
            { Stat.Endurance, 0 },
            { Stat.Intelligence, 0 },
            { Stat.Fortitude, 0 },
            { Stat.Agility, 0 }
        };
    }

    private void InitMoves()
    {
        Moves = new List<Move>();
        // Add moves starting from the highest learnable level
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
    }

    public void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Strength, CalculateNonHpStat(Base.Strength, StatPerformanceValues[Stat.Strength]) },
            { Stat.Endurance, CalculateNonHpStat(Base.Endurance, StatPerformanceValues[Stat.Endurance]) },
            { Stat.Intelligence, CalculateNonHpStat(Base.Intelligence, StatPerformanceValues[Stat.Intelligence]) },
            { Stat.Fortitude, CalculateNonHpStat(Base.Fortitude, StatPerformanceValues[Stat.Fortitude]) },
            { Stat.Agility, CalculateNonHpStat(Base.Agility, StatPerformanceValues[Stat.Agility]) }
        };

        int previousMaxHp = MaxHp;
        MaxHp = CalculateMaxHp(Base.HP, StatPerformanceValues[Stat.HP]);
        // Adjust current HP based on new max HP
        Hp = Mathf.Clamp(Hp + (MaxHp - previousMaxHp), 0, MaxHp);
        if (MaxHp > previousMaxHp)
        {
            OnHPChanged?.Invoke();
        }
    }

    private int CalculateNonHpStat(int baseStat, float performance)
    {
        return Mathf.FloorToInt((((2f * baseStat) + (performance / 4f)) * Level / 100f) + NON_HP_BASE_ADD);
    }

    private int CalculateMaxHp(int baseHp, float performance)
    {
        return Mathf.FloorToInt((((2f * baseHp) + (performance / 4f)) * Level / 100f) + Level + HP_LEVEL_ADD);
    }

    public void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>
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
        int baseStat = Stats.ContainsKey(stat) ? Stats[stat] : 0;
        int boost = StatBoosts.ContainsKey(stat) ? StatBoosts[stat] : 0;
        float[] boostMultipliers = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        return boost >= 0 ? Mathf.FloorToInt(baseStat * boostMultipliers[boost]) : Mathf.FloorToInt(baseStat / boostMultipliers[-boost]);
    }

    public void GainPvs(Dictionary<Stat, float> pvGained)
    {
        // Iterate over a copy to safely modify the dictionary during iteration.
        foreach (KeyValuePair<Stat, float> kvp in StatPerformanceValues.ToArray())
        {
            // Only apply gain if this stat's performance is below the per-stat max and overall total is below global max.
            if (kvp.Value < GlobalSettings.Instance.MaxPvPerStat && GetTotalPvs() < GlobalSettings.Instance.MaxPvs)
            {
                pvGained[kvp.Key] = Mathf.Clamp(pvGained[kvp.Key], 0, GlobalSettings.Instance.MaxPvs - GetTotalPvs());
                StatPerformanceValues[kvp.Key] = Mathf.Clamp(StatPerformanceValues[kvp.Key] + pvGained[kvp.Key], 0, GlobalSettings.Instance.MaxPvPerStat);
            }
        }
    }

    public float GetTotalPvs()
    {
        return StatPerformanceValues.Values.Sum();
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (StatBoost statBoost in statBoosts)
        {
            ApplySingleBoost(statBoost.Stat, statBoost.Boost);
        }
    }

    private void ApplySingleBoost(Stat stat, int boost)
    {
        int currentBoost = StatBoosts.ContainsKey(stat) ? StatBoosts[stat] : 0;
        bool isPositiveChange = boost > 0;
        string suffix = Base.Name.EndsWith("s") ? "'" : "'s";

        if ((isPositiveChange && currentBoost == MAX_STAT_BOOST) || (!isPositiveChange && currentBoost == MIN_STAT_BOOST))
        {
            string direction = isPositiveChange ? "higher" : "lower";
            AddStatusEvent($"{suffix} {stat} won't go any {direction}!");
            return;
        }

        StatBoosts[stat] = Mathf.Clamp(currentBoost + boost, MIN_STAT_BOOST, MAX_STAT_BOOST);
        string descriptor = (Mathf.Abs(boost) >= 3) ? "severely " : (Mathf.Abs(boost) == 2) ? "harshly " : "";
        string result = isPositiveChange ? "rose" : "fell";
        AddStatusEvent(StatusEventType.StatBoost, $"{suffix} {stat} {descriptor}{result}!", boost);
    }

    public bool CheckForLevelUp()
    {
        if (Level >= GlobalSettings.Instance.MaxLevel)
        {
            return false;
        }

        if (Exp >= Base.GetExpForLevel(Level + 1))
        {
            _level++;
            CalculateStats();
            return true;
        }
        return false;
    }

    public float GetNormalizedExp()
    {
        int currExp = Base.GetExpForLevel(Level);
        int nextExp = Base.GetExpForLevel(Level + 1);
        float normalized = (float)(Exp - currExp) / (nextExp - currExp);
        return Mathf.Clamp01(normalized);
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.FirstOrDefault(x => x.Level == Level);
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count >= BattlerBase.MaxMoveCount)
        {
            return;
        }

        Moves.Add(new Move(moveToLearn));
        AudioManager.Instance.PlaySFX(AudioID.MoveLearned);
    }

    public bool HasMove(MoveBase move)
    {
        return Moves.Any(m => m.Base == move);
    }

    public bool HasType(BattlerType type)
    {
        return Base.Type1 == type || Base.Type2 == type;
    }

    public Move GetRandomMove()
    {
        List<Move> movesWithSP = Moves.Where(static x => x.Sp > 0).ToList();
        if (movesWithSP.Count == 0)
        {
            return null;
        }

        int index = UnityEngine.Random.Range(0, movesWithSP.Count);
        return movesWithSP[index];
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

    public DamageDetails TakeDamage(Move move, Battler attacker, Condition weather)
    {
        // One-hit KO move handling
        if (move.Base.OneHitKO.IsOneHitKO)
        {
            int oneHitDamage = Hp;
            DecreaseHP(oneHitDamage);
            return new DamageDetails { TypeEffectiveness = 1f, Critical = 1f, Defeated = false };
        }

        float critMultiplier = CalculateCriticalMultiplier(move, attacker);
        float typeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, Base.Type1) *
                                  TypeChart.GetEffectiveness(move.Base.Type, Base.Type2);
        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;
        int damage = CalculateDamageOutput(move, attacker, critMultiplier, typeEffectiveness, weatherMod);

        DamageDetails details = new()
        {
            TypeEffectiveness = typeEffectiveness,
            Critical = critMultiplier,
            TotalDamageDealt = damage,
            ActualDamageDealt = Mathf.Min(damage, Hp),
            Defeated = false
        };

        DecreaseHP(damage);
        return details;
    }

    private float CalculateCriticalMultiplier(Move move, Battler attacker)
    {
        if (move.Base.CritBehavior == CritBehavior.NeverCrits)
        {
            return 1f;
        }

        if (move.Base.CritBehavior == CritBehavior.AlwaysCrits)
        {
            return 1.5f;
        }

        int critChance = (move.Base.CritBehavior == CritBehavior.HighCritRatio) ? 1 : 0;
        float[] critChances = new float[] { 4.167f, 12.5f, 50f, 100f };

        return UnityEngine.Random.value * 100f <= critChances[Mathf.Clamp(critChance, 0, 3)] ? 1.5f : 1f;
    }

    private int CalculateDamageOutput(Move move, Battler attacker, float crit, float typeEff, float weatherMod)
    {
        float attackStat = (move.Base.Category == MoveCategory.Magical) ? attacker.Intelligence : attacker.Strength;
        float defenseStat = (move.Base.Category == MoveCategory.Magical) ? Fortitude : Endurance;
        float randomMod = UnityEngine.Random.Range(0.85f, 1f);
        float modifier = randomMod * typeEff * crit * weatherMod;
        float baseDamage = ((2f * attacker.Level) + 10f) / 250f * move.Base.Power * ((attackStat / defenseStat) + 2f);
        int damage = Mathf.FloorToInt(baseDamage * modifier);

        if (IsGuarding)
        {
            damage = Mathf.FloorToInt(damage / 2f);
        }

        return Mathf.Max(damage, 1);
    }

    public void TakeRecoilDamage(int damage)
    {
        damage = Mathf.Max(damage, 1);
        AddStatusEvent(StatusEventType.Damage, " was damaged by recoil!", damage);
    }

    public void DrainHealth(int heal, string targetName)
    {
        heal = Mathf.Max(heal, 1);
        AddStatusEvent(StatusEventType.Heal, $" drained health from {targetName}!", heal);
    }

    public bool OnStartOfTurn()
    {
        bool canAct = true;
        foreach (ConditionID key in Statuses.Keys.ToList())
        {
            ConditionStatus status = Statuses[key];
            Condition condition = status.Condition;
            int timer = status.Timer;

            if (condition.OnBeginningOfTurnTimed != null)
            {
                (bool canActResult, int newTimer) = condition.OnBeginningOfTurnTimed(this, timer);
                if (newTimer <= 0)
                {
                    RemoveCondition(key);
                }
                else
                {
                    Statuses[key] = new ConditionStatus(condition, newTimer);
                }

                if (!canActResult)
                {
                    canAct = false;
                }
            }
            else if (condition.OnBeginningofTurn != null)
            {
                if (!condition.OnBeginningofTurn(this))
                {
                    canAct = false;
                }
            }
        }
        return canAct;
    }

    public void OnEndOfTurn()
    {
        foreach (KeyValuePair<ConditionID, ConditionStatus> kvp in Statuses.ToList())
        {
            kvp.Value.Condition.OnEndOfTurn?.Invoke(this);
        }
    }

    public void OnBattleOver()
    {
        VolatileStatuses.Clear();
        ResetStatBoosts();
        CalculateStats();
    }

    public void AddCondition(ConditionID conditionId, bool isVolatile = false)
    {
        if (conditionId == ConditionID.None)
        {
            return;
        }

        Dictionary<ConditionID, ConditionStatus> targetStatuses = isVolatile ? VolatileStatuses : Statuses;
        if (targetStatuses.ContainsKey(conditionId))
        {
            return;
        }

        Condition condition = ConditionsDB.Conditions[conditionId];
        int timer = condition.OnStartTimed != null ? condition.OnStartTimed(this) : 0;
        targetStatuses.Add(conditionId, new ConditionStatus(condition, timer));

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
            _ = Statuses.Remove(conditionId);
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

    public void AddStatusEvent(StatusEventType type, string message, int? value = null)
    {
        StatusChanges.Enqueue(new StatusEvent(type, message, value));
    }

    public void AddStatusEvent(string message)
    {
        AddStatusEvent(StatusEventType.Text, message, null);
    }

    public void UpdateAffinityLevel(int affinity)
    {
        AffinityLevel = Mathf.Clamp(AffinityLevel + affinity, 0, 6);
    }

    public BattlerSaveData GetSaveData()
    {
        return new BattlerSaveData
        {
            Name = Base.Name,
            Hp = Hp,
            Level = Level,
            Exp = Exp,
            Statuses = Statuses.Select(static s => new ConditionSaveData
            {
                ConditionId = s.Key,
                Timer = s.Value.Timer
            }).ToList(),
            Moves = Moves.Select(static m => m.GetSaveData()).ToList(),
            StatPerformanceValues = StatPerformanceValues.Select(static s => new StatPV
            {
                Stat = s.Key,
                Pv = s.Value
            }).ToList()
        };
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
        Message = message;
        Value = value;
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