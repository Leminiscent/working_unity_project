using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]
public class MonsterBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite sprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    // Base Stats
    [SerializeField] int baseMaxHP;
    [SerializeField] int baseMaxMp;
    [SerializeField] int baseStrength;
    [SerializeField] int baseAgility;
    [SerializeField] int baseIntelligence;
    [SerializeField] int baseStamina;
    [SerializeField] int baseLuck;

    // Growth Rates
    public float HpGrowthRate => baseMaxHP * 0.05f;
    public float MpGrowthRate => baseMaxMp * 0.03f;
    public float StrengthGrowthRate => baseStrength * 0.04f;
    public float AgilityGrowthRate => baseAgility * 0.04f;
    public float IntelligenceGrowthRate => baseIntelligence * 0.05f;
    public float StaminaGrowthRate => baseStamina * 0.04f;
    public float LuckGrowthRate => baseLuck * 0.03f;

    // Stats
    public int MaxHP { get; private set; }
    public int MaxMp { get; private set; }
    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public int Intelligence { get; private set; }
    public int Stamina { get; private set; }
    public int Luck { get; private set; }

    // Derived Stats
    public int Attack => Strength * 2;
    public int Accuracy => Agility + (Luck / 2);
    public int MagicAttack => Intelligence * 2;
    public int MagicAccuracy => Intelligence + (Luck / 2);
    public int Defense => Stamina * 2;
    public int Evasion => Agility + Luck;
    public int MagicDefense => Intelligence + Stamina;
    public int MagicEvasion => Agility + (Luck / 2);

    // Method to Level Up and Recalculate Stats
    public void LevelUp(int level)
    {
        StatGrowth growth = new StatGrowth();
        MaxHP = growth.CalculateHP(baseMaxHP, HpGrowthRate, level);
        MaxMp = growth.CalculateMP(baseMaxMp, MpGrowthRate, level);
        Strength = growth.CalculateStrength(baseStrength, StrengthGrowthRate, level);
        Agility = growth.CalculateAgility(baseAgility, AgilityGrowthRate, level);
        Intelligence = growth.CalculateIntelligence(baseIntelligence, IntelligenceGrowthRate, level);
        Stamina = growth.CalculateStamina(baseStamina, StaminaGrowthRate, level);
        Luck = growth.CalculateLuck(baseLuck, LuckGrowthRate, level);
    }
}

public enum MonsterType
{
    Beast,
    Dragon,
    Fiend,
    Flan,
    Flying,
    Humanoid,
    Machine,
    Plant,
    Stone,
    Undead,
    None
}