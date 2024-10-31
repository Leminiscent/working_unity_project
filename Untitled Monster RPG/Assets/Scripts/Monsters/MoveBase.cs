using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject
{
    // Attributes
    [SerializeField] private new string name;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private MonsterType type;
    [SerializeField] private MoveCategory category;
    [SerializeField] private MoveEffects effects;
    [SerializeField] private List<SecondaryEffects> secondaryEffects;
    [SerializeField] private CritBehavior critBehavior;
    [SerializeField] private RecoilMoveEffect recoil = new RecoilMoveEffect();
    [SerializeField] private int drainPercentage = 0;
    [SerializeField] private OneHitKOMoveEffect oneHitKO = new OneHitKOMoveEffect();
    [SerializeField] private MoveTarget target;
    [SerializeField] private Vector2Int hitRange;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private bool alwaysHits;
    [SerializeField] private int sP;
    [SerializeField] private int priority;
    [SerializeField] private AudioClip sound;

    // Properties
    public int GetHitCount()
    {
        int hitCount;

        if (hitRange == Vector2Int.zero)
        {
            hitCount = 1;
        }
        else
        {
            hitCount = hitRange.y == 0 ? hitRange.x : Random.Range(hitRange.x, hitRange.y + 1);
        }

        return hitCount;
    }

    public string Name => name;
    public string Description => description;
    public MonsterType Type => type;
    public CritBehavior CritBehavior => critBehavior;
    public RecoilMoveEffect Recoil => recoil;
    public int DrainPercentage => drainPercentage;
    public OneHitKOMoveEffect OneHitKO => oneHitKO;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHits => alwaysHits;
    public int SP => sP;
    public int Priority => priority;
    public MoveCategory Category => category;
    public MoveEffects Effects => effects;
    public List<SecondaryEffects> SecondaryEffects => secondaryEffects;
    public MoveTarget Target => target;
    public AudioClip Sound => sound;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] private List<StatBoost> boosts;
    [SerializeField] private ConditionID status;
    [SerializeField] private ConditionID volatileStatus;
    [SerializeField] private ConditionID weather;

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
    public ConditionID VolatileStatus => volatileStatus;
    public ConditionID Weather => weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] private int chance;
    [SerializeField] private MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

[System.Serializable]
public class RecoilMoveEffect
{
    public RecoilType recoilType;
    public int recoilDamage = 0;
}

[System.Serializable]
public class OneHitKOMoveEffect
{
    public bool isOneHitKO;
    public bool lowerOddsException;
    public MonsterType immunityType;
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
    Self
}

public enum CritBehavior
{
    none,
    HighCritRatio,
    AlwaysCrits,
    NeverCrits
}

public enum RecoilType
{
    none,
    RecoilByMaxHP,
    RecoilByCurrentHP,
    RecoilByDamage
}