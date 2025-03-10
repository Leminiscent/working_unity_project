using System.Collections.Generic;
using UnityEngine;

public class BattlerStorage : MonoBehaviour, ISavable
{
    private const int NUMBER_OF_DEPOTS = 10;
    private const int NUMBER_OF_SLOTS = 48; // 6 * 8
    private Battler[,] _depots = new Battler[NUMBER_OF_DEPOTS, NUMBER_OF_SLOTS];

    public int NumberOfDepots => NUMBER_OF_DEPOTS;
    public int NumberOfSlots => NUMBER_OF_SLOTS;

    public void AddBattler(Battler battler, int depotIndex, int slotIndex)
    {
        _depots[depotIndex, slotIndex] = battler;
    }

    public void RemoveBattler(int depotIndex, int slotIndex)
    {
        _depots[depotIndex, slotIndex] = null;
    }

    public Battler GetBattler(int depotIndex, int slotIndex)
    {
        return _depots[depotIndex, slotIndex];
    }

    public void AddBattlerToFirstEmptySlot(Battler battler)
    {
        for (int depotIndex = 0; depotIndex < NUMBER_OF_DEPOTS; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < NUMBER_OF_SLOTS; slotIndex++)
            {
                if (_depots[depotIndex, slotIndex] == null)
                {
                    _depots[depotIndex, slotIndex] = battler;
                    return;
                }
            }
        }
    }

    public static BattlerStorage GetPlayerStorage()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattlerStorage>();
    }

    public object CaptureState()
    {
        DepotSaveData saveData = new()
        {
            DepotSlots = new List<DepotSlotSaveData>()
        };

        for (int depotIndex = 0; depotIndex < NUMBER_OF_DEPOTS; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < NUMBER_OF_SLOTS; slotIndex++)
            {
                if (_depots[depotIndex, slotIndex] != null)
                {
                    DepotSlotSaveData depotSlot = new()
                    {
                        BattlerData = _depots[depotIndex, slotIndex].GetSaveData(),
                        DepotIndex = depotIndex,
                        SlotIndex = slotIndex
                    };

                    saveData.DepotSlots.Add(depotSlot);
                }
            }
        }

        return saveData;
    }

    public void RestoreState(object state)
    {
        DepotSaveData saveData = state as DepotSaveData;

        for (int depotIndex = 0; depotIndex < NUMBER_OF_DEPOTS; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < NUMBER_OF_SLOTS; slotIndex++)
            {
                _depots[depotIndex, slotIndex] = null;
            }
        }

        foreach (DepotSlotSaveData slot in saveData.DepotSlots)
        {
            _depots[slot.DepotIndex, slot.SlotIndex] = new Battler(slot.BattlerData);
        }
    }
}

[System.Serializable]
public class DepotSaveData
{
    public List<DepotSlotSaveData> DepotSlots;
}

[System.Serializable]
public class DepotSlotSaveData
{
    public BattlerSaveData BattlerData;
    public int DepotIndex;
    public int SlotIndex;
}