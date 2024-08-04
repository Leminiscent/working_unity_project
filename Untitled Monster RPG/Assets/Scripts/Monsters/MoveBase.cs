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

    // Base Stats
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;

    // Properties
    public string Name => name;
    public string Description => description;
    public MonsterType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pp;
}
