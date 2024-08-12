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
    [SerializeField] MoveTarget target;

    // Base Stats
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pP;

    // Properties
    public string Name => name;
    public string Description => description;
    public MonsterType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pP;
    public MoveCategory Category => category;
    public MoveEffects Effects => effects;
    public MoveTarget Target => target;
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;

    public List<StatBoost> Boosts => boosts;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

public enum MoveTarget
{
    Enemy,
    Self
}