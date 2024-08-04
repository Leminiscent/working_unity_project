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
    [SerializeField] int baseMaxHp;
    [SerializeField] int baseMaxMp;
    [SerializeField] int baseStrength;
    [SerializeField] int baseAgility;
    [SerializeField] int baseIntelligence;
    [SerializeField] int baseStamina;
    [SerializeField] int baseLuck;

    // Properties
    public string Name => name;
    public string Description => description;
    public Sprite Sprite => sprite;
    public MonsterType Type1 => type1;
    public MonsterType Type2 => type2;
    public int MaxHp => baseMaxHp;
    public int MaxMp => baseMaxMp;
    public int Strength => baseStrength;
    public int Agility => baseAgility;
    public int Intelligence => baseIntelligence;
    public int Stamina => baseStamina;
    public int Luck => baseLuck;

    // Growth Rates
    public float HpGrowthRate => baseMaxHp * 0.05f;
    public float MpGrowthRate => baseMaxMp * 0.03f;
    public float StrengthGrowthRate => baseStrength * 0.04f;
    public float AgilityGrowthRate => baseAgility * 0.04f;
    public float IntelligenceGrowthRate => baseIntelligence * 0.05f;
    public float StaminaGrowthRate => baseStamina * 0.04f;
    public float LuckGrowthRate => baseLuck * 0.03f;
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