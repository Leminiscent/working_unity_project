using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<MonsterEncounterRecord> wildMonsters;
    [SerializeField] List<MonsterEncounterRecord> wildWaterMonsters;

    [HideInInspector]
    [SerializeField] int totalChance = 0;

    [HideInInspector]
    [SerializeField] int totalChanceWater = 0;

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
        totalChanceWater = -1;

        if (wildMonsters.Count > 0)
        {
            totalChance = 0;
            foreach (var record in wildMonsters)
            {
                record.chanceLower = totalChance;
                record.chanceUpper = totalChance + record.spawnChance;
                totalChance += record.spawnChance;
            }
        }

        if (wildWaterMonsters.Count > 0)
        {
            totalChanceWater = 0;
            foreach (var record in wildWaterMonsters)
            {
                record.chanceLower = totalChanceWater;
                record.chanceUpper = totalChanceWater + record.spawnChance;
                totalChanceWater += record.spawnChance;
            }
        }
    }

    public Monster GetRandomWildMonster(BattleTrigger trigger)
    {
        var monsterList = (trigger == BattleTrigger.Ground) ? wildMonsters : wildWaterMonsters;
        int randVal = Random.Range(1, 101);
        var monsterRecord = monsterList.First(m => randVal >= m.chanceLower && randVal <= m.chanceUpper);
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

    public int chanceLower { get; set; }
    public int chanceUpper { get; set; }
}