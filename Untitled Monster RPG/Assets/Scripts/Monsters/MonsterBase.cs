using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new monster")]

public class MonsterBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite sprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    // Base Stats
    [SerializeField] int maxHP;
    [SerializeField] int maxMp;
    [SerializeField] int strength;
    [SerializeField] int agility;
    [SerializeField] int intelligence;
    [SerializeField] int stamina;
    [SerializeField] int luck;
    [SerializeField] int attack;
    [SerializeField] int accuracy;
    [SerializeField] int defense;
    [SerializeField] int evasion;
    [SerializeField] int magicDefense;
    [SerializeField] int magicEvasion;
}

public enum MonsterType
{
    None,
    Beast,
    Dragon,
    Fiend,
    Flan,
    Flying,
    Humanoid,
    Machine,
    Plant,
    Stone,
    Undead
}