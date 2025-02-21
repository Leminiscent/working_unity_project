using System;
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
    [SerializeField] private Rarity _rarity;

    [Header("Stat Weights")]
    [SerializeField] private float _totalStatsWeight;
    [SerializeField] private float _hpWeight;
    [SerializeField] private float _strengthWeight;
    [SerializeField] private float _enduranceWeight;
    [SerializeField] private float _intelligenceWeight;
    [SerializeField] private float _fortitudeWeight;
    [SerializeField] private float _agilityWeight;

    [Header("Moves")]
    [SerializeField] private List<LearnableMove> _learnableMoves = new();
    [SerializeField] private List<MoveBase> _learnableBySkillBook = new();

    [Header("Transformations")]
    [SerializeField] private List<Transformation> _transformations = new();

    [Header("Recruitment")]
    [SerializeField] private List<RecruitmentQuestion> _recruitmentQuestions = new();

    [Header("Drops")]
    [SerializeField] private DropTable _dropTable;

    [HideInInspector]
    [SerializeField]
    private float _sumOfWeights;

    private int _hp;
    private int _strength;
    private int _endurance;
    private int _intelligence;
    private int _fortitude;
    private int _agility;

    private Vector2Int _baseGp;

    private static readonly Dictionary<Rarity, (int min, int max)> _rarityStatRanges = new()
    {
        { Rarity.Common,     (30, 306) },
        { Rarity.Uncommon,   (307, 612) },
        { Rarity.Rare,       (613, 918) },
        { Rarity.Epic,       (919, 1224) },
        { Rarity.Legendary,  (1225, 1530) }
    };

    private static readonly Dictionary<Rarity, float> _rarityMultipliers = new()
    {
        { Rarity.Common, 1.0f },
        { Rarity.Uncommon, 1.2f },
        { Rarity.Rare, 1.5f },
        { Rarity.Epic, 1.8f },
        { Rarity.Legendary, 2.0f }
    };

    public string Name => _name;
    public string Description => _description;
    public Sprite Sprite => _sprite;
    public MonsterType Type1 => _type1;
    public MonsterType Type2 => _type2;
    public bool IsDualType => _type2 != MonsterType.None;
    public Rarity Rarity => _rarity;

    public int HP => _hp;
    public int Strength => _strength;
    public int Endurance => _endurance;
    public int Intelligence => _intelligence;
    public int Fortitude => _fortitude;
    public int Agility => _agility;
    public int TotalStats => _hp + _strength + _endurance + _intelligence + _fortitude + _agility;

    public Vector2Int BaseGp => _baseGp;

    public Dictionary<Stat, float> PvYield => new()
    {
        { Stat.HP, _hp * 0.01f },
        { Stat.Strength, _strength * 0.01f },
        { Stat.Endurance, _endurance * 0.01f },
        { Stat.Intelligence, _intelligence * 0.01f },
        { Stat.Fortitude, _fortitude * 0.01f },
        { Stat.Agility, _agility * 0.01f }
    };

    public List<LearnableMove> LearnableMoves => _learnableMoves;
    public List<MoveBase> LearnableBySkillBook => _learnableBySkillBook;
    public static int MaxMoveCount { get; } = 4;

    public List<Transformation> Transformations => _transformations;

    public int BaseExp => Mathf.RoundToInt(TotalStats * 0.25f);
    public int ExpYield
    {
        get
        {
            float rarityMultiplier = _rarityMultipliers[Rarity];
            return Mathf.RoundToInt(BaseExp * rarityMultiplier);
        }
    }
    public GrowthRate GrowthRate => AttributeCalculator.CalculateGrowthRate(Rarity, TotalStats, IsDualType);

    public int RecruitRate => AttributeCalculator.CalculateRecruitRate(Rarity, GrowthRate, TotalStats, IsDualType);
    public List<RecruitmentQuestion> RecruitmentQuestions => _recruitmentQuestions;

    public DropTable DropTable => _dropTable;

    private void OnValidate()
    {
        ValidateRarity();
        CalculateStats();
        CalculateBaseGP();
    }

    private void ValidateRarity()
    {
        if (!_rarityStatRanges.ContainsKey(_rarity))
        {
            Debug.LogError($"Rarity {_rarity} does not have a defined stat range.");
        }
    }

    private void CalculateStats()
    {
        if (!_rarityStatRanges.TryGetValue(_rarity, out (int min, int max) statRange))
        {
            Debug.LogError($"Cannot calculate stats due to undefined rarity {_rarity}.");
            return;
        }

        int totalStats = CalculateTotalStats(statRange.min, statRange.max);
        AssignIndividualStats(totalStats);
    }

    private int CalculateTotalStats(int minTotal, int maxTotal)
    {
        float totalRange = maxTotal - minTotal;
        int totalStats = Mathf.RoundToInt(_totalStatsWeight * totalRange) + minTotal;
        return Mathf.Clamp(totalStats, minTotal, maxTotal);
    }

    private void AssignIndividualStats(int totalStats)
    {
        float[] weights = { _hpWeight, _strengthWeight, _enduranceWeight, _intelligenceWeight, _fortitudeWeight, _agilityWeight };
        _sumOfWeights = 0f;

        foreach (float weight in weights)
        {
            _sumOfWeights += weight;
        }

        _hp = Mathf.Clamp(Mathf.RoundToInt(weights[0] * totalStats), 5, 255);
        _strength = Mathf.Clamp(Mathf.RoundToInt(weights[1] * totalStats), 5, 255);
        _endurance = Mathf.Clamp(Mathf.RoundToInt(weights[2] * totalStats), 5, 255);
        _intelligence = Mathf.Clamp(Mathf.RoundToInt(weights[3] * totalStats), 5, 255);
        _fortitude = Mathf.Clamp(Mathf.RoundToInt(weights[4] * totalStats), 5, 255);
        _agility = Mathf.Clamp(Mathf.RoundToInt(weights[5] * totalStats), 5, 255);
    }

    private void CalculateBaseGP()
    {
        float rarityMultiplier = _rarityMultipliers.ContainsKey(Rarity) ? _rarityMultipliers[Rarity] : 1.0f;
        float statsMultiplier = TotalStats * 0.05f;

        _baseGp = new Vector2Int(Mathf.RoundToInt((1 * rarityMultiplier) + statsMultiplier), Mathf.RoundToInt((3 * rarityMultiplier) + statsMultiplier));
    }

    public int GetExpForLevel(int level)
    {
        switch (GrowthRate)
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
                throw new ArgumentOutOfRangeException();
        }
    }

    public int CalculateGpYield()
    {
        return UnityEngine.Random.Range(_baseGp.x, _baseGp.y + 1);
    }
}

[Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase _moveBase;
    [SerializeField] private int _level;

    public MoveBase Base => _moveBase;
    public int Level => _level;
}

[Serializable]
public class Transformation
{
    [SerializeField] private MonsterBase _transformsInto;
    [SerializeField] private int _requiredLevel;
    [SerializeField] private TransformationItem _requiredItem;

    public MonsterBase TransformsInto => _transformsInto;
    public int RequiredLevel => _requiredLevel;
    public TransformationItem RequiredItem => _requiredItem;
}

[Serializable]
public class RecruitmentQuestion
{
    [SerializeField] private string _questionText;
    [SerializeField] private List<RecruitmentAnswer> _answers = new();

    public string QuestionText => _questionText;
    public List<RecruitmentAnswer> Answers => _answers;
}

[Serializable]
public class RecruitmentAnswer
{
    [SerializeField] private string _answerText;
    [SerializeField] private int _affinityScore;

    public string AnswerText => _answerText;
    public int AffinityScore => _affinityScore;
}

[Serializable]
public class DropTable
{
    [SerializeField] private List<ItemDrop> _itemDrops = new();

    public List<ItemDrop> ItemDrops => _itemDrops;
}

[Serializable]
public class ItemDrop
{
    [SerializeField] private ItemBase _item;
    [SerializeField] private Vector2Int _quantityRange;
    [SerializeField] private float _dropChance;

    public ItemBase Item => _item;
    public Vector2Int QuantityRange => _quantityRange;
    public float DropChance => _dropChance;
}

public class TypeChart
{
    private static float[,] _chart =
    {
        //             NOR   FIR   WAT   THU   PLA   ICE   FOR   POI   EAR   WIN   MIN   INS   SPI   DRA   DAR   LIG
        /*Normal*/   { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f },
        /*Fire*/     { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Water*/    { 1.0f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Thunder*/  { 1.0f, 1.0f, 2.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Plant*/    { 1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 2.0f, 0.5f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Ice*/      { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f },
        /*Force*/    { 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 2.0f, 0.5f },
        /*Poison*/   { 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f },
        /*Earth*/    { 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Wind*/     { 1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Mind*/     { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f },
        /*Insect*/   { 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.5f, 1.0f, 0.5f, 2.0f, 1.0f, 0.5f, 1.0f, 2.0f, 0.5f },
        /*Spirit*/   { 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f },
        /*Dragon*/   { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.0f },
        /*Dark*/     { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 2.0f, 1.0f, 0.5f, 0.5f },
        /*Light*/    { 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f }
    };

    public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
        {
            return 1f;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return _chart[row, col];
    }
}

public class AttributeCalculator
{
    private static readonly Dictionary<Rarity, int> _rarityPoints = new()
    {
        { Rarity.Common, 1 },
        { Rarity.Uncommon, 2 },
        { Rarity.Rare, 3 },
        { Rarity.Epic, 4 },
        { Rarity.Legendary, 5 }
    };

    private static readonly Dictionary<GrowthRate, int> _growthRatePoints = new()
    {
        { GrowthRate.Erratic, 1 },
        { GrowthRate.Fast, 2 },
        { GrowthRate.MediumFast, 3 },
        { GrowthRate.MediumSlow, 4 },
        { GrowthRate.Slow, 5 },
        { GrowthRate.Fluctuating, 6 }
    };

    private static readonly List<(int MaxValue, int Points)> _statsPointsRanges = new()
    {
        (128, 1),
        (256, 2),
        (384, 3),
        (512, 4),
        (int.MaxValue, 5)
    };

    private static readonly List<(int MaxPoints, GrowthRate Rate)> _growthRateMappings = new()
    {
        (2, GrowthRate.Erratic),
        (4, GrowthRate.Fast),
        (6, GrowthRate.MediumFast),
        (8, GrowthRate.MediumSlow),
        (10, GrowthRate.Slow),
        (int.MaxValue, GrowthRate.Fluctuating)
    };

    private const int SINGLE_TYPE_POINTS = 1;
    private const int DUAL_TYPE_POINTS = 2;
    private const int MIN_TOTAL_POINTS = 4;
    private const int MAX_TOTAL_POINTS = 18;

    public static GrowthRate CalculateGrowthRate(Rarity rarity, int totalStats, bool isDualType)
    {
        int rarityPoints = GetRarityPoints(rarity);
        int statsPoints = GetStatsPoints(totalStats);
        int typingPoints = isDualType ? DUAL_TYPE_POINTS : SINGLE_TYPE_POINTS;

        int totalPoints = rarityPoints + statsPoints + typingPoints;

        return MapPointsToGrowthRate(totalPoints);
    }

    public static int CalculateRecruitRate(Rarity rarity, GrowthRate growthRate, int totalStats, bool isDualType)
    {
        int rarityPoints = GetRarityPoints(rarity);
        int growthRatePoints = GetGrowthRatePoints(growthRate);
        int statsPoints = GetStatsPoints(totalStats);
        int typingPoints = isDualType ? DUAL_TYPE_POINTS : SINGLE_TYPE_POINTS;

        int totalPoints = rarityPoints + growthRatePoints + statsPoints + typingPoints;

        totalPoints = Mathf.Clamp(totalPoints, MIN_TOTAL_POINTS, MAX_TOTAL_POINTS);

        float rate = (float)(MAX_TOTAL_POINTS - totalPoints) / (MAX_TOTAL_POINTS - MIN_TOTAL_POINTS) * 255f;
        rate = Mathf.Clamp(rate, 3f, 255f);

        return Mathf.RoundToInt(rate);
    }

    private static int GetRarityPoints(Rarity rarity)
    {
        return _rarityPoints.TryGetValue(rarity, out int points) ? points : 1;
    }

    private static int GetGrowthRatePoints(GrowthRate growthRate)
    {
        return _growthRatePoints.TryGetValue(growthRate, out int points) ? points : 2;
    }

    private static int GetStatsPoints(int totalStats)
    {
        foreach ((int maxValue, int points) in _statsPointsRanges)
        {
            if (totalStats <= maxValue)
            {
                return points;
            }
        }
        return 1; // Default fallback
    }

    private static GrowthRate MapPointsToGrowthRate(int totalPoints)
    {
        foreach ((int maxPoints, GrowthRate rate) in _growthRateMappings)
        {
            if (totalPoints <= maxPoints)
            {
                return rate;
            }
        }

        return GrowthRate.MediumFast; // Default fallback
    }
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

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
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

public enum GrowthRate
{
    Erratic,
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating
}