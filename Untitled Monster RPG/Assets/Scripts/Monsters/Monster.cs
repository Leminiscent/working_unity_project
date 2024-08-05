using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster
{
    MonsterBase _base;
    int level;

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Monster(MonsterBase mBase, int mLevel)
    {
        _base = mBase;
        level = mLevel;
        HP = _base.MaxHp;

        Moves = new List<Move>();
        foreach (var move in _base.LearnableMoves)
        {
            if (move.Level <= level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4)
            {
                break;
            }
        }
    }

    public int MaxHp => Mathf.FloorToInt((_base.MaxHp * level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((_base.Attack * level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((_base.Defense * level) / 100f) + 5;
    public int SpAttack => Mathf.FloorToInt((_base.SpAttack * level) / 100f) + 5;
    public int SpDefense => Mathf.FloorToInt((_base.SpDefense * level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((_base.Speed * level) / 100f) + 5;
}