using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<MonsterEncounterRecord> wildMonsters;
    [SerializeField] private BattleTrigger terrain;

    [HideInInspector]
    [SerializeField] private int totalChance = 0;

    public BattleTrigger Terrain => terrain;

    private void OnValidate()
    {
        CalculateSpawnChance();
    }

    private void Start()
    {
        CalculateSpawnChance();
    }

    private void CalculateSpawnChance()
    {
        totalChance = -1;

        if (wildMonsters.Count > 0)
        {
            totalChance = 0;
            foreach (MonsterEncounterRecord record in wildMonsters)
            {
                record.ChanceLower = totalChance;
                record.ChanceUpper = totalChance + record.spawnChance;
                totalChance += record.spawnChance;
            }
        }
    }

    public Monster GetRandomWildMonster()
    {
        List<MonsterEncounterRecord> monsterList = wildMonsters;
        int randVal = Random.Range(1, 101);
        MonsterEncounterRecord monsterRecord = monsterList.First(m => randVal >= m.ChanceLower && randVal <= m.ChanceUpper);
        Vector2Int levelRange = monsterRecord.levelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        Monster wildMonster = new Monster(monsterRecord.monster, level);

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