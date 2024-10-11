using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStorage : MonoBehaviour
{
    const int numberOfDepots = 10;
    const int numberOfSlots = 48; // 6 * 8
    Monster[,] depots = new Monster[numberOfDepots, numberOfSlots];

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
}
