using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class MapArea : MonoBehaviour
{
    [SerializeField] private List<BattlerEncounterRecord> _rogueBattlers;
    [SerializeField, HideInInspector] private int _totalChance = 0;
    [field: SerializeField, FormerlySerializedAs("_terrain")] public BattleTrigger Terrain { get; private set; }
    [field: SerializeField] public WeatherConditionID Weather { get; private set; }

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
        if (_rogueBattlers == null || _rogueBattlers.Count == 0)
        {
            _totalChance = -1;
            return;
        }

        _totalChance = 0;
        foreach (BattlerEncounterRecord record in _rogueBattlers)
        {
            record.ChanceLower = _totalChance;
            record.ChanceUpper = _totalChance + record.SpawnChance;
            _totalChance += record.SpawnChance;
        }
    }

    public Battler GetRandomRogueBattler()
    {
        if (_rogueBattlers == null || _rogueBattlers.Count == 0)
        {
            Debug.LogWarning("No rogue battlers configured in MapArea.");
            return null;
        }

        int randVal = Random.Range(1, 101);
        BattlerEncounterRecord battlerRecord = _rogueBattlers.FirstOrDefault(b => randVal >= b.ChanceLower && randVal <= b.ChanceUpper);
        if (battlerRecord == null)
        {
            Debug.LogWarning("No battler record matched the random value. Returning first battler as fallback.");
            battlerRecord = _rogueBattlers[0];
        }

        Vector2Int levelRange = battlerRecord.LevelRange;
        int level = levelRange.y == 0 ? levelRange.x : Random.Range(levelRange.x, levelRange.y + 1);
        Battler rogueBattler = new(battlerRecord.Battler, level);

        rogueBattler.InitBattler();
        return rogueBattler;
    }

    public List<Battler> GetRandomRogueBattlers(int count)
    {
        List<Battler> battlers = new();

        for (int i = 0; i < count; i++)
        {
            Battler battler = GetRandomRogueBattler();
            if (battler != null)
            {
                battlers.Add(battler);
            }
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