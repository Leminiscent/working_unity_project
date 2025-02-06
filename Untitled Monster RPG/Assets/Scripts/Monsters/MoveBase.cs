using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject
{
    // Attributes
    [SerializeField] private string _name;
    [TextArea]
    [SerializeField] private string _description;
    [SerializeField] private MonsterType _type;
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
    [SerializeField] private AudioClip _sound;

    // Properties
    public int GetHitCount()
    {
        int hitCount = _hitRange == Vector2Int.zero ? 1 : _hitRange.y == 0 ? _hitRange.x : Random.Range(_hitRange.x, _hitRange.y + 1);
        return hitCount;
    }

    public string Name => _name;
    public string Description => _description;
    public MonsterType Type => _type;
    public CritBehavior CritBehavior => _critBehavior;
    public RecoilMoveEffect Recoil => _recoil;
    public int DrainPercentage => _drainPercentage;
    public OneHitKOMoveEffect OneHitKO => _oneHitKO;
    public int Power => _power;
    public int Accuracy => _accuracy;
    public bool AlwaysHits => _alwaysHits;
    public int SP => _sP;
    public int Priority => _priority;
    public MoveCategory Category => _category;
    public MoveEffects Effects => _effects;
    public List<SecondaryEffects> SecondaryEffects => _secondaryEffects;
    public MoveTarget Target => _target;
    public AudioClip Sound => _sound;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] private List<StatBoost> _boosts;
    [SerializeField] private ConditionID _status;
    [SerializeField] private ConditionID _volatileStatus;
    [SerializeField] private ConditionID _weather;

    public List<StatBoost> Boosts => _boosts;
    public ConditionID Status => _status;
    public ConditionID VolatileStatus => _volatileStatus;
    public ConditionID Weather => _weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] private int _chance;
    [SerializeField] private MoveTarget _target;

    public int Chance => _chance;
    public MoveTarget Target => _target;
}

[System.Serializable]
public class StatBoost
{
    public Stat Stat;
    public int Boost;
}

[System.Serializable]
public class RecoilMoveEffect
{
    public RecoilType RecoilType;
    public int RecoilDamage = 0;
}

[System.Serializable]
public class OneHitKOMoveEffect
{
    public bool IsOneHitKO;
    public bool LowerOddsException;
    public MonsterType ImmunityType;
}

public enum MoveCategory
{
    Physical,
    Magical,
    Status
}

public enum MoveTarget
{
    Enemy,
    Ally,
    AllEnemies,
    AllAllies,
    Self,
    Others
}

public enum CritBehavior
{
    None,
    HighCritRatio,
    AlwaysCrits,
    NeverCrits
}

public enum RecoilType
{
    None,
    RecoilByMaxHP,
    RecoilByCurrentHP,
    RecoilByDamage
}
