using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<MonsterEncounterRecord> wildMonsters;
    [SerializeField] BattleTrigger terrain;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    public BattleTrigger Terrain => terrain;

    private void OnValidate()
    {
        CalculateSpawnChance();
    }

    private void Start()
    {
        CalculateSpawnChance();
    }

    void CalculateSpawnChance()
    {
        totalChance = -1;

        if (wildMonsters.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildMonsters)
            {
                record.ChanceLower = totalChance;
                record.ChanceUpper = totalChance + record.spawnChance;
                totalChance += record.spawnChance;
            }
        }
    }

    public Monster GetRandomWildMonster()
    {
        var monsterList = wildMonsters;
        int randVal = Random.Range(1, 101);
        var monsterRecord = monsterList.First(m => randVal >= m.ChanceLower && randVal <= m.ChanceUpper);
        var levelRange = monsterRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        var wildMonster = new Monster(monsterRecord.monster, level);

        wildMonster.Init();
        return wildMonster;
    }
}

[System.Serializable]
public class MonsterEncounterRecord
{
    public MonsterBase monster;
    public Vector2Int levelRange;
    public int spawnChance;

    public int ChanceLower { get; set; }
    public int ChanceUpper { get; set; }
}