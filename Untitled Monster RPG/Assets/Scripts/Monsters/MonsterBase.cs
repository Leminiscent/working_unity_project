using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]
public class MonsterBase : ScriptableObject
{
    [Header("Basic Details")]
    [SerializeField] new string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite sprite;
    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    [Header("Stats")]
    [SerializeField] int hp;
    [SerializeField] int strength;
    [SerializeField] int endurance;
    [SerializeField] int intelligence;
    [SerializeField] int fortitude;
    [SerializeField] int agility;

    [Header("Performance Value Yield")]
    [SerializeField] int hpPvYield = 0;
    [SerializeField] int strengthPvYield = 0;
    [SerializeField] int endurancePvYield = 0;
    [SerializeField] int intelligencePvYield = 0;
    [SerializeField] int fortitudePvYield = 0;
    [SerializeField] int agilityPvYield = 0;

    [Header("Moves")]
    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableBySkillBook;

    [Header("Transformations")]
    [SerializeField] List<Transformation> transformations;

    [Header("Experience")]
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [Header("Recruitment")]
    [SerializeField] int recruitRate;
    [SerializeField] List<RecruitmentQuestion> recruitmentQuestions;

    [Header("Drops")]
    [SerializeField] DropTable dropTable;

    public int GetExpForLevel(int level)
    {
        switch (growthRate)
        {
            case GrowthRate.Erratic:
                if (level < 50)
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (100 - level) / 50);
                if (level < 68)
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (150 - level) / 100);
                if (level < 98)
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (1911 - 10 * level) / 3 / 500);
                return Mathf.FloorToInt(Mathf.Pow(level, 3) * (160 - level) / 100);
            case GrowthRate.Fast:
                return Mathf.FloorToInt(4 * Mathf.Pow(level, 3) / 5);
            case GrowthRate.MediumFast:
                return Mathf.FloorToInt(Mathf.Pow(level, 3));
            case GrowthRate.MediumSlow:
                return Mathf.FloorToInt(6f / 5 * Mathf.Pow(level, 3) - 15 * Mathf.Pow(level, 2) + 100 * level - 140);
            case GrowthRate.Slow:
                return Mathf.FloorToInt(5 * Mathf.Pow(level, 3) / 4);
            case GrowthRate.Fluctuating:
                if (level < 15)
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 1) / 3 + 24) / 50);
                if (level < 36)
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level + 14) / 50);
                return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level / 2 + 32) / 50);
        }
        return -1;
    }

    public string Name => name;
    public string Description => description;
    public Sprite Sprite => sprite;
    public MonsterType Type1 => type1;
    public MonsterType Type2 => type2;
    public int HP => hp;
    public int Strength => strength;
    public int Endurance => endurance;
    public int Intelligence => intelligence;
    public int Fortitude => fortitude;
    public int Agility => agility;
    public Dictionary<Stat, int> PvYield => new()
    {
        { Stat.HP, hpPvYield },
        { Stat.Strength, strengthPvYield },
        { Stat.Endurance, endurancePvYield },
        { Stat.Intelligence, intelligencePvYield },
        { Stat.Fortitude, fortitudePvYield },
        { Stat.Agility, agilityPvYield }
    };
    public List<LearnableMove> LearnableMoves => learnableMoves;
    public List<MoveBase> LearnableBySkillBook => learnableBySkillBook;
    public static int MaxMoveCount { get; set; } = 4;
    public List<Transformation> Transformations => transformations;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;
    public int RecruitRate => recruitRate;
    public List<RecruitmentQuestion> RecruitmentQuestions => recruitmentQuestions;
    public DropTable DropTable => dropTable;
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base => moveBase;
    public int Level => level;
}

[System.Serializable]
public class Transformation
{
    [SerializeField] MonsterBase transformsInto;
    [SerializeField] int requiredLevel;
    [SerializeField] TransformationItem requiredItem;

    public MonsterBase TransformsInto => transformsInto;
    public int RequiredLevel => requiredLevel;
    public TransformationItem RequiredItem => requiredItem;
}

[System.Serializable]
public class RecruitmentQuestion
{
    [SerializeField] string questionText;
    [SerializeField] List<RecruitmentAnswer> answers;

    public string QuestionText => questionText;
    public List<RecruitmentAnswer> Answers => answers;
}

[System.Serializable]
public class RecruitmentAnswer
{
    [SerializeField] string answerText;
    [SerializeField] int affinityScore;

    public string AnswerText => answerText;
    public int AffinityScore => affinityScore;
}

[System.Serializable]
public class DropTable
{
    [SerializeField] Vector2Int gpDropped;
    [SerializeField] List<ItemDrop> itemDrops;

    public Vector2Int GpDropped => gpDropped;
    public List<ItemDrop> ItemDrops => itemDrops;
}

[System.Serializable]
public class ItemDrop
{
    [SerializeField] ItemBase item;
    [SerializeField] Vector2Int quantityRange;
    [SerializeField] float dropChance;
    public ItemBase Item => item;
    public Vector2Int QuantityRange => quantityRange;
    public float DropChance => dropChance;
}

public enum MonsterType
{
    None,
    Normal,
    Fire,
    Water,
    Thunder,
    Plant,
    Ice,
    Force,
    Poison,
    Earth,
    Wind,
    Mind,
    Insect,
    Stone,
    Spirit,
    Dragon,
    Dark,
    Metal,
    Light
}

public enum GrowthRate
{
    Erratic,
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating
}

public enum Stat
{
    HP,
    Strength,
    Endurance,
    Intelligence,
    Fortitude,
    Agility,
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart =
    {
        //                         NOR   FIR   WAT   THU   PLA   ICE   FOR   POI   EAR   WIN   MIN   INS   STO   SPI   DRA   DAR   MET   LIG
        /*Normal*/   new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.0f, 1.0f, 1.0f, 0.5f, 1.0f },
        /*Fire*/     new float[] { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 2.0f, 1.0f },
        /*Water*/    new float[] { 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f },
        /*Thunder*/  new float[] { 1.0f, 1.0f, 2.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f },
        /*Plant*/    new float[] { 1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 2.0f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f },
        /*Ice*/      new float[] { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f },
        /*Force*/    new float[] { 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f, 0.5f, 0.5f, 2.0f, 0.0f, 1.0f, 2.0f, 2.0f, 0.5f },
        /*Poison*/   new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 2.0f },
        /*Earth*/    new float[] { 1.0f, 2.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.0f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f },
        /*Wind*/     new float[] { 1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f },
        /*Mind*/     new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.5f, 1.0f },
        /*Insect*/   new float[] { 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 2.0f, 0.5f, 0.5f },
        /*Stone*/    new float[] { 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f },
        /*Spirit*/   new float[] { 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Dragon*/   new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.0f },
        /*Dark*/     new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f },
        /*Metal*/    new float[] { 1.0f, 0.5f, 0.5f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 2.0f },
        /*Light*/    new float[] { 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 0.5f, 1.0f }
    };

    public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}