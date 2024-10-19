using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Monster/Create new move")]
public class MoveBase : ScriptableObject
{
    // Attributes
    [SerializeField] new string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] MonsterType type;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaryEffects;
    [SerializeField] CritBehavior critBehavior;
    [SerializeField] RecoilMoveEffect recoil = new RecoilMoveEffect();
    [SerializeField] MoveTarget target;
    [SerializeField] Vector2Int hitRange;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHits;
    [SerializeField] int sP;
    [SerializeField] int priority;
    [SerializeField] AudioClip sound;

    // Properties
    public int GetHitCount()
    {
        int hitCount;

        if (hitRange == Vector2Int.zero)
        {
            hitCount = 1;
        }
        else if (hitRange.y == 0)
        {
            hitCount = hitRange.x;
        }
        else
        {
            hitCount = Random.Range(hitRange.x, hitRange.y + 1);
        }

        return hitCount;
    }

    public string Name => name;
    public string Description => description;
    public MonsterType Type => type;
    public CritBehavior CritBehavior => critBehavior;
    public RecoilMoveEffect Recoil => recoil;
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
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;
    [SerializeField] ConditionID weather;

    public List<StatBoost> Boosts => boosts;
    public ConditionID Status => status;
    public ConditionID VolatileStatus => volatileStatus;
    public ConditionID Weather => weather;
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

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