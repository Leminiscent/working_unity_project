using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Battler", menuName = "Battler/Create new battler")]
public class BattlerBase : ScriptableObject
{
    [Header("Basic Details")]
    [SerializeField] private string _name;
    [TextArea]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private BattlerType _type1;
    [SerializeField] private BattlerType _type2;
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

    [Header("Overworld Sprites")]
    [SerializeField] private List<Sprite> _walkDownSprites;
    [SerializeField] private List<Sprite> _walkUpSprites;
    [SerializeField] private List<Sprite> _walkRightSprites;
    [SerializeField] private List<Sprite> _walkLeftSprites;

    // Calculated stat values
    [SerializeField, HideInInspector] private int _hp;
    [SerializeField, HideInInspector] private int _strength;
    [SerializeField, HideInInspector] private int _endurance;
    [SerializeField, HideInInspector] private int _intelligence;
    [SerializeField, HideInInspector] private int _fortitude;
    [SerializeField, HideInInspector] private int _agility;
    [SerializeField, HideInInspector] private Vector2Int _baseGp;

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
    public BattlerType Type1 => _type1;
    public BattlerType Type2 => _type2;
    public bool IsDualType => _type2 != BattlerType.None;
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

    public List<Sprite> WalkDownSprites => _walkDownSprites;
    public List<Sprite> WalkUpSprites => _walkUpSprites;
    public List<Sprite> WalkRightSprites => _walkRightSprites;
    public List<Sprite> WalkLeftSprites => _walkLeftSprites;

    private void OnValidate()
    {
        ValidateRarity();
        RecalculateStats();
    }

    private void ValidateRarity()
    {
        if (!_rarityStatRanges.ContainsKey(_rarity))
        {
            Debug.LogError($"Rarity {_rarity} does not have a defined stat range.");
        }
    }

    /// <summary>
    /// Recalculates total stats, individual stat values, and base GP.
    /// </summary>
    private void RecalculateStats()
    {
        // Calculate total stats based on rarity ranges and weight
        int totalStats = StatCalculator.CalculateTotalStats(_totalStatsWeight, _rarity, _rarityStatRanges);

        // Prepare weights for individual stats in the order: HP, Strength, Endurance, Intelligence, Fortitude, Agility
        float[] weights = { _hpWeight, _strengthWeight, _enduranceWeight, _intelligenceWeight, _fortitudeWeight, _agilityWeight };

        // Calculate individual stats using normalized weights
        Dictionary<string, int> stats = StatCalculator.AssignIndividualStats(totalStats, weights);

        _hp = stats["HP"];
        _strength = stats["Strength"];
        _endurance = stats["Endurance"];
        _intelligence = stats["Intelligence"];
        _fortitude = stats["Fortitude"];
        _agility = stats["Agility"];

        // Calculate base GP using the total stats and rarity multiplier
        _baseGp = StatCalculator.CalculateBaseGP(totalStats, _rarityMultipliers[Rarity]);
    }

    public int GetExpForLevel(int level)
    {
        return ExperienceCalculator.GetExpForLevel(GrowthRate, level);
    }

    public int CalculateGpYield()
    {
        return UnityEngine.Random.Range(_baseGp.x, _baseGp.y + 1);
    }
}


public static class StatCalculator
{
    // Named constants for clamping values.
    public const int MIN_STAT_VALUE = 5;
    public const int MAX_STAT_VALUE = 255;

    /// <summary>
    /// Calculates total stats based on a weight factor and rarity's defined stat range.
    /// </summary>
    public static int CalculateTotalStats(float totalStatsWeight, Rarity rarity, Dictionary<Rarity, (int min, int max)> rarityStatRanges)
    {
        if (!rarityStatRanges.TryGetValue(rarity, out (int min, int max) statRange))
        {
            Debug.LogError($"Cannot calculate total stats due to undefined rarity {rarity}.");
            return 0;
        }

        int totalRange = statRange.max - statRange.min;
        int totalStats = Mathf.RoundToInt(totalStatsWeight * totalRange) + statRange.min;
        return Mathf.Clamp(totalStats, statRange.min, statRange.max);
    }

    /// <summary>
    /// Distributes the total stats among individual stats according to provided weights.
    /// Weights are normalized to ensure a proportional distribution.
    /// </summary>
    public static Dictionary<string, int> AssignIndividualStats(int totalStats, float[] weights)
    {
        float weightSum = 0f;
        foreach (float weight in weights)
        {
            weightSum += weight;
        }

        if (weightSum <= 0)
        {
            Debug.LogWarning("Sum of stat weights is zero or negative. Defaulting to equal distribution.");
            weightSum = weights.Length;
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 1f;
            }
        }

        Dictionary<string, int> stats = new()
        {
            ["HP"] = Mathf.Clamp(Mathf.RoundToInt(weights[0] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE),
            ["Strength"] = Mathf.Clamp(Mathf.RoundToInt(weights[1] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE),
            ["Endurance"] = Mathf.Clamp(Mathf.RoundToInt(weights[2] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE),
            ["Intelligence"] = Mathf.Clamp(Mathf.RoundToInt(weights[3] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE),
            ["Fortitude"] = Mathf.Clamp(Mathf.RoundToInt(weights[4] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE),
            ["Agility"] = Mathf.Clamp(Mathf.RoundToInt(weights[5] / weightSum * totalStats), MIN_STAT_VALUE, MAX_STAT_VALUE)
        };

        return stats;
    }

    /// <summary>
    /// Calculates the base GP (Gold Points or similar) based on total stats and a rarity multiplier.
    /// </summary>
    public static Vector2Int CalculateBaseGP(int totalStats, float rarityMultiplier)
    {
        float statsMultiplier = totalStats * 0.05f;
        int gpMin = Mathf.RoundToInt((1 * rarityMultiplier) + statsMultiplier);
        int gpMax = Mathf.RoundToInt((3 * rarityMultiplier) + statsMultiplier);
        return new Vector2Int(gpMin, gpMax);
    }
}

public static class ExperienceCalculator
{
    /// <summary>
    /// Calculates the experience points required for a given level based on the growth rate.
    /// </summary>
    public static int GetExpForLevel(GrowthRate growthRate, int level)
    {
        if (level == 1)
        {
            return 0;
        }

        switch (growthRate)
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
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (((level + 1) / 3f) + 24) / 50);
                }
                if (level < 36)
                {
                    return Mathf.FloorToInt(Mathf.Pow(level, 3) * (level + 14) / 50);
                }
                return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level / 2f) + 32) / 50);

            default:
                throw new ArgumentOutOfRangeException(nameof(growthRate), "Unsupported growth rate.");
        }
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
    [SerializeField] private BattlerBase _transformsInto;
    [SerializeField] private int _requiredLevel;
    [SerializeField] private TransformationItem _requiredItem;

    public BattlerBase TransformsInto => _transformsInto;
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
        //             NOR   FIR   WAT   THU   NAT   ICE   FOR   EAR   WIN   OCC   DAR   LIG
        /*Normal*/   { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 1.0f },
        /*Fire*/     { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Water*/    { 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f },
        /*Thunder*/  { 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 2.0f, 0.5f, 1.0f, 1.0f },
        /*Nature*/   { 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Ice*/      { 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f },
        /*Force*/    { 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Earth*/    { 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Wind*/     { 1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Occult*/   { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 0.0f },
        /*Dark*/     { 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f },
        /*Light*/    { 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f }
    };

    public static float GetEffectiveness(BattlerType attackType, BattlerType defenseType)
    {
        if (attackType == BattlerType.None || defenseType == BattlerType.None)
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
        int rarityPts = GetRarityPoints(rarity);
        int statsPts = GetStatsPoints(totalStats);
        int typingPts = isDualType ? DUAL_TYPE_POINTS : SINGLE_TYPE_POINTS;

        int totalPoints = rarityPts + statsPts + typingPts;
        return MapPointsToGrowthRate(totalPoints);
    }

    public static int CalculateRecruitRate(Rarity rarity, GrowthRate growthRate, int totalStats, bool isDualType)
    {
        int rarityPts = GetRarityPoints(rarity);
        int growthRatePts = GetGrowthRatePoints(growthRate);
        int statsPts = GetStatsPoints(totalStats);
        int typingPts = isDualType ? DUAL_TYPE_POINTS : SINGLE_TYPE_POINTS;

        int totalPoints = rarityPts + growthRatePts + statsPts + typingPts;
        totalPoints = Mathf.Clamp(totalPoints, MIN_TOTAL_POINTS, MAX_TOTAL_POINTS);

        float rate = (float)(MAX_TOTAL_POINTS - totalPoints) / (MAX_TOTAL_POINTS - MIN_TOTAL_POINTS) * 255f;
        rate = Mathf.Clamp(rate, 3f, 255f);

        return Mathf.RoundToInt(rate);
    }

    private static int GetRarityPoints(Rarity rarity)
    {
        return _rarityPoints.TryGetValue(rarity, out int pts) ? pts : 1;
    }

    private static int GetGrowthRatePoints(GrowthRate growthRate)
    {
        return _growthRatePoints.TryGetValue(growthRate, out int pts) ? pts : 2;
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
        return 1;
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
        return GrowthRate.MediumFast;
    }
}

public enum BattlerType
{
    None,
    Normal,
    Fire,
    Water,
    Thunder,
    Nature,
    Ice,
    Force,
    Earth,
    Wind,
    Occult,
    Dark,
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