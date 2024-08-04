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

    public int MaxHP => _base.MaxHp + (int)(_base.HpGrowthRate * Mathf.Pow(level - 1, 2));
    public int MaxMp => _base.MaxMp + (int)(_base.MpGrowthRate * Mathf.Log(level));
    public int Strength => _base.Strength + (int)(_base.StrengthGrowthRate * (level - 1));
    public int Agility => _base.Agility + (int)(_base.AgilityGrowthRate * (level - 1));
    public int Intelligence => _base.Intelligence * (int)Mathf.Pow(1 + _base.IntelligenceGrowthRate, level - 1);
    public int Stamina => _base.Stamina + (int)(_base.StaminaGrowthRate * (level - 1));
    public int Luck => _base.Luck + (int)(_base.LuckGrowthRate * Mathf.Log(level));
    public int Attack => Strength * 2;
    public int Accuracy => Agility + (Luck / 2);
    public int MagicAttack => Intelligence * 2;
    public int MagicAccuracy => Intelligence + (Luck / 2);
    public int Defense => Stamina * 2;
    public int Evasion => Agility + Luck;
    public int MagicDefense => Intelligence + Stamina;
    public int MagicEvasion => Agility + (Luck / 2);
    public int Speed => Agility + (Luck / 4);
}