using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a move that can be performed by a battler during battle.
/// </summary>
[CreateAssetMenu(fileName = "Move", menuName = "Battler/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string _name;
    [TextArea, SerializeField] private string _description;
    [SerializeField] private BattlerType _type;
    [SerializeField] private MoveCategory _category;
    [SerializeField] private MoveEffects _effects;
    [SerializeField] private List<SecondaryEffects> _secondaryEffects;
    [SerializeField] private CritBehavior _critBehavior;
    [SerializeField] private RecoilMoveEffect _recoil = new();
    [SerializeField] private int _drainPercentage = 0;
    [SerializeField] private OneHitKOMoveEffect _oneHitKO = new();
    [SerializeField] private MoveTarget _target;
    [SerializeField] private Vector2Int _hitRange;
    [SerializeField] private int _power;
    [SerializeField] private int _accuracy;
    [SerializeField] private bool _alwaysHits;
    [SerializeField] private int _sP;
    [SerializeField] private int _priority;
    [SerializeField] private AudioClip _castSound;
    [SerializeField] private AudioClip _effectSound;
    [SerializeField] private List<Sprite> _castAnimationSprites;
    [SerializeField] private List<Sprite> _effectAnimationSprites;

    /// <summary>
    /// Returns the number of times the move should hit.
    /// If the hit range is zero, returns 1. If the upper bound is zero, returns the lower bound;
    /// otherwise returns a random number between the lower and upper bounds (inclusive).
    /// </summary>
    public int GetHitCount()
    {
        int hitCount = _hitRange == Vector2Int.zero
            ? 1
            : _hitRange.y == 0
                ? _hitRange.x
                : Random.Range(_hitRange.x, _hitRange.y + 1);
        return hitCount;
    }

    /// <summary>
    /// The name of the move.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The description of the move.
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// The primary type of the move.
    /// </summary>
    public BattlerType Type => _type;

    /// <summary>
    /// The critical hit behavior of the move.
    /// </summary>
    public CritBehavior CritBehavior => _critBehavior;

    /// <summary>
    /// The recoil effect applied when using the move.
    /// </summary>
    public RecoilMoveEffect Recoil => _recoil;

    /// <summary>
    /// The percentage of damage drained from the target.
    /// </summary>
    public int DrainPercentage => _drainPercentage;

    /// <summary>
    /// The one-hit KO effect of the move.
    /// </summary>
    public OneHitKOMoveEffect OneHitKO => _oneHitKO;

    /// <summary>
    /// The power (damage value) of the move.
    /// </summary>
    public int Power => _power;

    /// <summary>
    /// The accuracy percentage of the move.
    /// </summary>
    public int Accuracy => _accuracy;

    /// <summary>
    /// Indicates whether the move always hits regardless of accuracy.
    /// </summary>
    public bool AlwaysHits => _alwaysHits;

    /// <summary>
    /// The SP cost to use the move.
    /// </summary>
    public int SP => _sP;

    /// <summary>
    /// The priority of the move.
    /// </summary>
    public int Priority => _priority;

    /// <summary>
    /// The category of the move (Physical, Magical, or Status).
    /// </summary>
    public MoveCategory Category => _category;

    /// <summary>
    /// The primary effects of the move.
    /// </summary>
    public MoveEffects Effects => _effects;

    /// <summary>
    /// The list of secondary effects for the move.
    /// </summary>
    public List<SecondaryEffects> SecondaryEffects => _secondaryEffects;

    /// <summary>
    /// The intended target of the move.
    /// </summary>
    public MoveTarget Target => _target;

    /// <summary>
    /// The sound to play when casting the move.
    /// </summary>
    public AudioClip CastSound => _castSound;

    /// <summary>
    /// The sound to play when the move's effect occurs.
    /// </summary>
    public AudioClip EffectSound => _effectSound;

    /// <summary>
    /// The list of sprites used for the casting animation.
    /// </summary>
    public List<Sprite> CastAnimationSprites => _castAnimationSprites;

    /// <summary>
    /// The list of sprites used for the effect animation.
    /// </summary>
    public List<Sprite> EffectAnimationSprites => _effectAnimationSprites;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] private List<StatBoost> _boosts;
    [SerializeField] private ConditionID _status;
    [SerializeField] private ConditionID _volatileStatus;
    [SerializeField] private ConditionID _weather;

    /// <summary>
    /// List of stat boosts applied by the move.
    /// </summary>
    public List<StatBoost> Boosts => _boosts;

    /// <summary>
    /// The condition status applied by the move.
    /// </summary>
    public ConditionID Status => _status;

    /// <summary>
    /// The volatile condition status applied by the move.
    /// </summary>
    public ConditionID VolatileStatus => _volatileStatus;

    /// <summary>
    /// The weather condition applied by the move.
    /// </summary>
    public ConditionID Weather => _weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] private int _chance;
    [SerializeField] private MoveTarget _target;

    /// <summary>
    /// The chance (percentage) for the secondary effect to occur.
    /// </summary>
    public int Chance => _chance;

    /// <summary>
    /// The target of the secondary effect.
    /// </summary>
    public MoveTarget Target => _target;
}

[System.Serializable]
public class StatBoost
{
    /// <summary>
    /// The stat that is boosted.
    /// </summary>
    public Stat Stat;

    /// <summary>
    /// The amount by which the stat is boosted.
    /// </summary>
    public int Boost;
}

[System.Serializable]
public class RecoilMoveEffect
{
    /// <summary>
    /// The type of recoil applied.
    /// </summary>
    public RecoilType RecoilType;

    /// <summary>
    /// The damage inflicted as recoil.
    /// </summary>
    public int RecoilDamage = 0;
}

[System.Serializable]
public class OneHitKOMoveEffect
{
    /// <summary>
    /// Indicates whether the move is a one-hit KO.
    /// </summary>
    public bool IsOneHitKO;

    /// <summary>
    /// Indicates whether a lower odds exception applies.
    /// </summary>
    public bool LowerOddsException;

    /// <summary>
    /// The battler type that is immune to this one-hit KO.
    /// </summary>
    public BattlerType ImmunityType;
}

/// <summary>
/// The category of a move.
/// </summary>
public enum MoveCategory
{
    Physical,
    Magical,
    Status
}

/// <summary>
/// Specifies the target of a move.
/// </summary>
public enum MoveTarget
{
    Enemy,
    Ally,
    AllEnemies,
    AllAllies,
    Self,
    Others
}

/// <summary>
/// Describes the critical hit behavior of a move.
/// </summary>
public enum CritBehavior
{
    None,
    HighCritRatio,
    AlwaysCrits,
    NeverCrits
}

/// <summary>
/// Defines the type of recoil applied by a move.
/// </summary>
public enum RecoilType
{
    None,
    RecoilByMaxHP,
    RecoilByCurrentHP,
    RecoilByDamage
}