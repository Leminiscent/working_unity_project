using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]
public class MonsterBase : ScriptableObject
{
    [Header("Basic Details")]
    [SerializeField] private string _name;
    [TextArea]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private MonsterType _type1;
    [SerializeField] private MonsterType _type2;

    [Header("Stats")]
    [SerializeField] private int _hp;
    [SerializeField] private int _strength;
    [SerializeField] private int _endurance;
    [SerializeField] private int _intelligence;
    [SerializeField] private int _fortitude;
    [SerializeField] private int _agility;

    [Header("Performance Value Yield")]
    [SerializeField] private int _hpPvYield = 0;
    [SerializeField] private int _strengthPvYield = 0;
    [SerializeField] private int _endurancePvYield = 0;
    [SerializeField] private int _intelligencePvYield = 0;
    [SerializeField] private int _fortitudePvYield = 0;
    [SerializeField] private int _agilityPvYield = 0;

    [Header("Moves")]
    [SerializeField] private List<LearnableMove> _learnableMoves;
    [SerializeField] private List<MoveBase> _learnableBySkillBook;

    [Header("Transformations")]
    [SerializeField] private List<Transformation> _transformations;

    [Header("Experience")]
    [SerializeField] private int _expYield;
    [SerializeField] private GrowthRate _growthRate;

    [Header("Recruitment")]
    [SerializeField] private int _recruitRate;
    [SerializeField] private List<RecruitmentQuestion> _recruitmentQuestions;

    [Header("Drops")]
    [SerializeField] private DropTable _dropTable;

    public string Name => _name;
    public string Description => _description;
    public Sprite Sprite => _sprite;
    public MonsterType Type1 => _type1;
    public MonsterType Type2 => _type2;
    public int HP => _hp;
    public int Strength => _strength;
    public int Endurance => _endurance;
    public int Intelligence => _intelligence;
    public int Fortitude => _fortitude;
    public int Agility => _agility;
    public Dictionary<Stat, int> PvYield => new()
    {
        { Stat.HP, _hpPvYield },
        { Stat.Strength, _strengthPvYield },
        { Stat.Endurance, _endurancePvYield },
        { Stat.Intelligence, _intelligencePvYield },
        { Stat.Fortitude, _fortitudePvYield },
        { Stat.Agility, _agilityPvYield }
    };
    public List<LearnableMove> LearnableMoves => _learnableMoves;
    public List<MoveBase> LearnableBySkillBook => _learnableBySkillBook;
    public static int MaxMoveCount { get; set; } = 4;
    public List<Transformation> Transformations => _transformations;
    public int ExpYield => _expYield;
    public GrowthRate GrowthRate => _growthRate;
    public int RecruitRate => _recruitRate;
    public List<RecruitmentQuestion> RecruitmentQuestions => _recruitmentQuestions;
    public DropTable DropTable => _dropTable;

    public int GetExpForLevel(int level)
    {
        switch (_growthRate)
        {
            case GrowthRate.Erratic:
                if (level < 50)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (100 - level) / 50);
                }

                if (level < 68)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (150 - level) / 100);
                }

                if (level < 98)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (1911 - (10 * level)) / 3 / 500);
                }

                return Mathf.FloorToInt(Mathf.Pow(level, 3) * (160 - level) / 100);
            case GrowthRate.Fast:
                return Mathf.FloorToInt(4 * Mathf.Pow(level, 3) / 5);
            case GrowthRate.MediumFast:
                return Mathf.FloorToInt(Mathf.Pow(level, 3));
            case GrowthRate.MediumSlow:
                return Mathf.FloorToInt((6f / 5 * Mathf.Pow(level, 3)) - (15 * Mathf.Pow(level, 2)) + (100 * level) - 140);
            case GrowthRate.Slow:
                return Mathf.FloorToInt(5 * Mathf.Pow(level, 3) / 4);
            case GrowthRate.Fluctuating:
                if (level < 15)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (((level + 1) / 3) + 24) / 50);
                }

                if (level < 36)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level + 14) / 50);
                }

                return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level / 2) + 32) / 50);
            default:
                break;
        }
        return -1;
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase _moveBase;
    [SerializeField] private int _level;

    public MoveBase Base => _moveBase;
    public int Level => _level;
}

[System.Serializable]
public class Transformation
{
    [SerializeField] private MonsterBase _transformsInto;
    [SerializeField] private int _requiredLevel;
    [SerializeField] private TransformationItem _requiredItem;

    public MonsterBase TransformsInto => _transformsInto;
    public int RequiredLevel => _requiredLevel;
    public TransformationItem RequiredItem => _requiredItem;
}

[System.Serializable]
public class RecruitmentQuestion
{
    [SerializeField] private string _questionText;
    [SerializeField] private List<RecruitmentAnswer> _answers;

    public string QuestionText => _questionText;
    public List<RecruitmentAnswer> Answers => _answers;
}

[System.Serializable]
public class RecruitmentAnswer
{
    [SerializeField] private string _answerText;
    [SerializeField] private int _affinityScore;

    public string AnswerText => _answerText;
    public int AffinityScore => _affinityScore;
}

[System.Serializable]
public class DropTable
{
    [SerializeField] private Vector2Int _gpDropped;
    [SerializeField] private List<ItemDrop> _itemDrops;

    public Vector2Int GpDropped => _gpDropped;
    public List<ItemDrop> ItemDrops => _itemDrops;
}

[System.Serializable]
public class ItemDrop
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private Vector2Int _quantityRange;
    [SerializeField] private float _dropChance;

    public ItemBase Item => _item;
    public Vector2Int QuantityRange => _quantityRange;
    public float DropChance => _dropChance;
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
    private static float[][] _chart =
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
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return _chart[row][col];
    }
}