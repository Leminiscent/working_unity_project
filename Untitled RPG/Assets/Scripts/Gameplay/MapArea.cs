using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<BattlerEncounterRecord> _rogueBattlers;
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

        if (_rogueBattlers.Count > 0)
        {
            _totalChance = 0;
            foreach (BattlerEncounterRecord record in _rogueBattlers)
            {
                record.ChanceLower = _totalChance;
                record.ChanceUpper = _totalChance + record.SpawnChance;
                _totalChance += record.SpawnChance;
            }
        }
    }

    public Battler GetRandomRogueBattler()
    {
        List<BattlerEncounterRecord> battlerList = _rogueBattlers;
        int randVal = Random.Range(1, 101);
        BattlerEncounterRecord battlerRecord = battlerList.First(b => randVal >= b.ChanceLower && randVal <= b.ChanceUpper);
        Vector2Int levelRange = battlerRecord.LevelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        Battler rogueBattler = new(battlerRecord.Battler, level);

        rogueBattler.InitNewBattler();
        return rogueBattler;
    }

    public List<Battler> GetRandomRogueBattlers(int count)
    {
        List<Battler> battlers = new();

        for (int i = 0; i < count; i++)
        {
            Battler battler = GetRandomRogueBattler();
            battlers.Add(battler);
        }
        return battlers;
    }
}

[System.Serializable]
public class BattlerEncounterRecord
{
    public BattlerBase Battler;
    public Vector2Int LevelRange;
    public int SpawnChance;
    public int ChanceLower { get; set; }
    public int ChanceUpper { get; set; }
}