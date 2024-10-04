using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStorage : MonoBehaviour
{
    Monster[,] depots = new Monster[10, 48];

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

    public static MonsterStorage GetPlayerStorage()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonsterStorage>();
    }
}
