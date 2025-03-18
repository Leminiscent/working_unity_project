using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Move", menuName = "Battler/Create new move")]
public class MoveBase : ScriptableObject
{
    [field: Header("Basic Details")]
    [field: SerializeField, FormerlySerializedAs("_name")] public string Name { get; private set; }
    [field: TextArea, SerializeField, FormerlySerializedAs("_description")] public string Description { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_type")] public BattlerType Type { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_category")] public MoveCategory Category { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_target")] public MoveTarget Target { get; private set; }

    [field: Header("Effects")]
    [field: SerializeField, FormerlySerializedAs("_effects")] public MoveEffects Effects { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_secondaryEffects")] public List<SecondaryEffects> SecondaryEffects { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_critBehavior")] public CritBehavior CritBehavior { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_recoil")] public RecoilMoveEffect Recoil { get; private set; } = new();
    [field: SerializeField, FormerlySerializedAs("_drainPercentage")] public int DrainPercentage { get; private set; } = 0;
    [field: SerializeField, FormerlySerializedAs("_oneHitKO")] public OneHitKOMoveEffect OneHitKO { get; private set; } = new();

    [Header("Stats")]
    [SerializeField] private Vector2Int _hitRange;
    [field: SerializeField, FormerlySerializedAs("_power")] public int Power { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_accuracy")] public int Accuracy { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_alwaysHits")] public bool AlwaysHits { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_sp")] public int SP { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_priority")] public int Priority { get; private set; }

    [field: Header("Animation & Sound")]
    [field: SerializeField, FormerlySerializedAs("_castSound")] public AudioClip CastSound { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_effectSound")] public AudioClip EffectSound { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_castAnimationSprites")] public List<Sprite> CastAnimationSprites { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_effectAnimationSprites")] public List<Sprite> EffectAnimationSprites { get; private set; }

    public int GetHitCount()
    {
        int hitCount = _hitRange == Vector2Int.zero
            ? 1
            : _hitRange.y == 0
                ? _hitRange.x
                : Random.Range(_hitRange.x, _hitRange.y + 1);
        return hitCount;
    }
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
    public BattlerType ImmunityType;
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