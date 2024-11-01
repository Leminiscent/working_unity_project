using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<MonsterEncounterRecord> _wildMonsters;
    [SerializeField] private BattleTrigger _terrain;

    [HideInInspector]
    [SerializeField] private int _totalChance = 0;

    public BattleTrigger Terrain => _terrain;

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
        _totalChance = -1;

        if (_wildMonsters.Count > 0)
        {
            _totalChance = 0;
            foreach (MonsterEncounterRecord record in _wildMonsters)
            {
                record.ChanceLower = _totalChance;
                record.ChanceUpper = _totalChance + record.SpawnChance;
                _totalChance += record.SpawnChance;
            }
        }
    }

    public Monster GetRandomWildMonster()
    {
        List<MonsterEncounterRecord> monsterList = _wildMonsters;
        int randVal = Random.Range(1, 101);
        MonsterEncounterRecord monsterRecord = monsterList.First(m => randVal >= m.ChanceLower && randVal <= m.ChanceUpper);
        Vector2Int levelRange = monsterRecord.LevelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        Monster wildMonster = new(monsterRecord.Monster, level);

        wildMonster.Init();
        return wildMonster;
    }
}

[System.Serializable]
public class MonsterEncounterRecord
{
    public MonsterBase Monster;
    public Vector2Int LevelRange;
    public int SpawnChance;
    public int ChanceLower { get; set; }
    public int ChanceUpper { get; set; }
}