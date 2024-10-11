using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Monster
{
    [SerializeField] MonsterBase _base;
    [SerializeField] int level;

    public Monster(MonsterBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }
    public MonsterBase Base => _base;
    public int Level => level;
    public int Exp { get; set; }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public int AffinityLevel { get; set; }
    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= MonsterBase.MaxMoveCount)
            {
                break;
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        Status = null;
        VolatileStatus = null;
        AffinityLevel = 0;
    }

    public Monster(MonsterSaveData saveData)
    {
        _base = MonsterDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();
        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoosts();
        VolatileStatus = null;
    }

    public MonsterSaveData GetSaveData()
    {
        var saveData = new MonsterSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            statusId = Status?.ID,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    void CalculateStats()
    {
        int prevMaxHp = MaxHp;

        MaxHp = Mathf.FloorToInt(Base.HP * Level / 100f) + 10 + Level;
        if (prevMaxHp != 0)
        {
            HP += MaxHp - prevMaxHp;
        }

        Stats = new Dictionary<Stat, int>()
        {
            { Stat.Strength, Mathf.FloorToInt(Base.Strength * Level / 100f) + 5 },
            { Stat.Endurance, Mathf.FloorToInt(Base.Endurance * Level / 100f) + 5 },
            { Stat.Intelligence, Mathf.FloorToInt(Base.Intelligence * Level / 100f) + 5 },
            { Stat.Fortitude, Mathf.FloorToInt(Base.Fortitude * Level / 100f) + 5 },
            { Stat.Agility, Mathf.FloorToInt(Base.Agility * Level / 100f) + 5 },
        };
    }

    void ResetStatBoosts()
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

    int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
        }
    }

    public bool CheckForLevelUp()
    {
        if (Exp >= Base.GetExpForLevel(Level + 1))
        {
            ++level;
            CalculateStats();
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if (Moves.Count > MonsterBase.MaxMoveCount) return;
        Moves.Add(new Move(moveToLearn));
    }

    public bool HasMove(MoveBase move)
    {
        return Moves.Count(m => m.Base == move) > 0;
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

    public void Heal()
    {
        HP = MaxHp;
        OnHPChanged?.Invoke();
        CureStatus();
    }

    public float GetNormalizedExp()
    {
        int currLevelExp = Base.GetExpForLevel(Level);
        int nextLevelExp = Base.GetExpForLevel(Level + 1);
        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);

        return Mathf.Clamp01(normalizedExp);
    }

    public int MaxHp { get; private set; }
    public int Strength => GetStat(Stat.Strength);
    public int Endurance => GetStat(Stat.Endurance);
    public int Intelligence => GetStat(Stat.Intelligence);
    public int Fortitude => GetStat(Stat.Fortitude);
    public int Agility => GetStat(Stat.Agility);

    public DamageDetails TakeDamage(Move move, Monster attacker, Condition weather)
    {
        float critical = 1f;

        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        float weatherMod = weather?.OnDamageModify?.Invoke(this, attacker, move) ?? 1f;
        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Defeated = false
        };
        float attack = (move.Base.Category == MoveCategory.Magical) ? attacker.Intelligence : attacker.Strength;
        float defense = (move.Base.Category == MoveCategory.Magical) ? Fortitude : Endurance;
        float modifiers = Random.Range(0.85f, 1f) * type * critical * weatherMod;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * (((float)attack / defense) + 2);
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHP(damage);

        return damageDetails;
    }

    public void DecreaseHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int heal)
    {
        HP = Mathf.Clamp(HP + heal, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void UpdateAffinityLevel(int affinity)
    {
        AffinityLevel = Mathf.Clamp(AffinityLevel + affinity, 0, 6);
    }

    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null) return;

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null) return;

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        var movesWithSP = Moves.Where(x => x.SP > 0).ToList();
        int randomIndex = Random.Range(0, movesWithSP.Count);

        return movesWithSP[randomIndex];
    }

    public bool OnStartOfTurn()
    {
        bool canPerformMove = true;

        if (Status?.OnBeginningofTurn != null)
        {
            if (!Status.OnBeginningofTurn(this))
            {
                canPerformMove = false;
            }
        }
        if (VolatileStatus?.OnBeginningofTurn != null)
        {
            if (!VolatileStatus.OnBeginningofTurn(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnEndOfTurn()
    {
        Status?.OnEndOfTurn?.Invoke(this);
        VolatileStatus?.OnEndOfTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Defeated { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class MonsterSaveData
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
}