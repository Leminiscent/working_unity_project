using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster
{
    MonsterBase _base;
    int level;

    public Monster(MonsterBase mBase, int mLevel)
    {
        _base = mBase;
        level = mLevel;
    }

    public int MaxHp => Mathf.FloorToInt((_base.MaxHp * level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((_base.Attack * level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((_base.Defense * level) / 100f) + 5;
    public int SpAttack => Mathf.FloorToInt((_base.SpAttack * level) / 100f) + 5;
    public int SpDefense => Mathf.FloorToInt((_base.SpDefense * level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((_base.Speed * level) / 100f) + 5;
}