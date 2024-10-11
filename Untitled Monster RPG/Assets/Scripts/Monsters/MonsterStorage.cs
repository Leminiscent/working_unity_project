using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStorage : MonoBehaviour, ISavable
{
    const int numberOfDepots = 10;
    const int numberOfSlots = 48; // 6 * 8
    Monster[,] depots = new Monster[numberOfDepots, numberOfSlots];

    public int NumberOfDepots => numberOfDepots;
    public int NumberOfSlots => numberOfSlots;

    public void AddMonster(Monster monster, int depotIndex, int slotIndex)
    {
        depots[depotIndex, slotIndex] = monster;
    }

    public void RemoveMonster(int depotIndex, int slotIndex)
    {
        depots[depotIndex, slotIndex] = null;
    }

    public Monster GetMonster(int depotIndex, int slotIndex)
    {
        return depots[depotIndex, slotIndex];
    }

    public void AddMonsterToFirstEmptySlot(Monster monster)
    {
        for (int depotIndex = 0; depotIndex < numberOfDepots; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
            {
                if (depots[depotIndex, slotIndex] == null)
                {
                    depots[depotIndex, slotIndex] = monster;
                    return;
                }
            }
        }
    }

    public static MonsterStorage GetPlayerStorage()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonsterStorage>();
    }

    public object CaptureState()
    {
        var saveData = new DepotSaveData()
        {
            depotSlots = new List<DepotSlotSaveData>()
        };

        for (int depotIndex = 0; depotIndex < numberOfDepots; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
            {
                if (depots[depotIndex, slotIndex] != null)
                {
                    var depotSlot = new DepotSlotSaveData()
                    {
                        monsterData = depots[depotIndex, slotIndex].GetSaveData(),
                        depotIndex = depotIndex,
                        slotIndex = slotIndex
                    };

                    saveData.depotSlots.Add(depotSlot);
                }
            }
        }

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as DepotSaveData;

        for (int depotIndex = 0; depotIndex < numberOfDepots; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < numberOfSlots; slotIndex++)
            {
                depots[depotIndex, slotIndex] = null;
            }
        }

        foreach (var slot in saveData.depotSlots)
        {
            depots[slot.depotIndex, slot.slotIndex] = new Monster(slot.monsterData);
        }
    }
}

[System.Serializable]
public class DepotSaveData
{
    public List<DepotSlotSaveData> depotSlots;
}

[System.Serializable]
public class DepotSlotSaveData
{
    public MonsterSaveData monsterData;
    public int depotIndex;
    public int slotIndex;
}