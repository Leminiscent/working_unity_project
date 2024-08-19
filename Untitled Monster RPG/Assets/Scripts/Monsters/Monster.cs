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
    public int StatusBuildup { get; set; }
    public Queue<string> StatusChanges { get; private set; }
    public bool HpChanged { get; set; }
    public int AffectionLevel { get; set; }
    public event System.Action OnStatusChanged;

    public void Init()
    {
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4)
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
        StatusBuildup = 0;

        AffectionLevel = 0;
    }

    void CalculateStats()
    {
        MaxHp = Mathf.FloorToInt(Base.MaxHp * Level / 100f) + 10 + Level;
        Stats = new Dictionary<Stat, int>()
        {
            { Stat.Attack, Mathf.FloorToInt(Base.Attack * Level / 100f) + 5 },
            { Stat.Defense, Mathf.FloorToInt(Base.Defense * Level / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt(Base.SpAttack * Level / 100f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt(Base.SpDefense * Level / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt(Base.Speed * Level / 100f) + 5 },
        };
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
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

    public int MaxHp { get; private set; }
    public int Attack => GetStat(Stat.Attack);
    public int Defense => GetStat(Stat.Defense);
    public int SpAttack => GetStat(Stat.SpAttack);
    public int SpDefense => GetStat(Stat.SpDefense);
    public int Speed => GetStat(Stat.Speed);

    public DamageDetails TakeDamage(Move move, Monster attacker)
    {
        float critical = 1f;

        if (Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Defeated = false
        };
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;
        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HpChanged = true;
    }

    public void UpdateAffectionLevel(int affection)
    {
        AffectionLevel = Mathf.Clamp(AffectionLevel + affection, 0, 6);
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
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        int randomIndex = Random.Range(0, movesWithPP.Count);

        return movesWithPP[randomIndex];
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
        StatusBuildup = 0;
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Defeated { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}