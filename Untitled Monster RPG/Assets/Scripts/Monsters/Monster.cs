using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster
{
    public MonsterBase Base { get; set; }
    public int Level { get; set; }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Monster(MonsterBase mBase, int mLevel)
    {
        Base = mBase;
        Level = mLevel;
        HP = MaxHp;

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
    }

    public int MaxHp => Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5;
    public int SpAttack => Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5;
    public int SpDefense => Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5;

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

        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Defeated = true;
        }

        return damageDetails;
    }

    public Move GetRandomMove()
    {
        int randomIndex = Random.Range(0, Moves.Count);
        return Moves[randomIndex];
    }
}

public class DamageDetails
{
    public bool Defeated { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}