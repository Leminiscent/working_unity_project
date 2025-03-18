using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages storage for battlers across multiple depots and slots, and implements saving/restoring its state.
/// </summary>
public class BattlerStorage : MonoBehaviour, ISavable
{
    private const int NUMBER_OF_DEPOTS = 10;
    private const int NUMBER_OF_SLOTS = 48; // 6 * 8

    private Battler[,] _depots = new Battler[NUMBER_OF_DEPOTS, NUMBER_OF_SLOTS];

    public int NumberOfDepots => NUMBER_OF_DEPOTS;
    public int NumberOfSlots => NUMBER_OF_SLOTS;

    /// <summary>
    /// Adds a battler to a specific depot and slot.
    /// </summary>
    /// <param name="battler">The battler to add.</param>
    /// <param name="depotIndex">The depot index.</param>
    /// <param name="slotIndex">The slot index.</param>
    public void AddBattler(Battler battler, int depotIndex, int slotIndex)
    {
        _depots[depotIndex, slotIndex] = battler;
    }

    /// <summary>
    /// Removes the battler from a specific depot and slot.
    /// </summary>
    /// <param name="depotIndex">The depot index.</param>
    /// <param name="slotIndex">The slot index.</param>
    public void RemoveBattler(int depotIndex, int slotIndex)
    {
        _depots[depotIndex, slotIndex] = null;
    }

    /// <summary>
    /// Retrieves the battler from a specified depot and slot.
    /// </summary>
    /// <param name="depotIndex">The depot index.</param>
    /// <param name="slotIndex">The slot index.</param>
    /// <returns>The battler if found; otherwise, null.</returns>
    public Battler GetBattler(int depotIndex, int slotIndex)
    {
        return _depots[depotIndex, slotIndex];
    }

    /// <summary>
    /// Adds a battler to the first empty slot found.
    /// </summary>
    /// <param name="battler">The battler to add.</param>
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

    /// <summary>
    /// Retrieves the BattlerStorage component from the PlayerController.
    /// </summary>
    /// <returns>The player's BattlerStorage.</returns>
    public static BattlerStorage GetPlayerStorage()
    {
        return FindObjectOfType<PlayerController>().GetComponent<BattlerStorage>();
    }

    /// <summary>
    /// Captures the current state of the battler storage.
    /// </summary>
    /// <returns>An object containing the save data.</returns>
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
                Battler battler = _depots[depotIndex, slotIndex];
                if (battler != null)
                {
                    DepotSlotSaveData depotSlot = new()
                    {
                        BattlerData = battler.GetSaveData(),
                        DepotIndex = depotIndex,
                        SlotIndex = slotIndex
                    };

                    saveData.DepotSlots.Add(depotSlot);
                }
            }
        }

        return saveData;
    }

    /// <summary>
    /// Restores the state of the battler storage from saved data.
    /// </summary>
    /// <param name="state">The saved state (expected to be of type DepotSaveData).</param>
    public void RestoreState(object state)
    {
        if (state is not DepotSaveData saveData)
        {
            Debug.LogError("Failed to restore BattlerStorage state: Invalid save data.");
            return;
        }

        // Clear all depots.
        for (int depotIndex = 0; depotIndex < NUMBER_OF_DEPOTS; depotIndex++)
        {
            for (int slotIndex = 0; slotIndex < NUMBER_OF_SLOTS; slotIndex++)
            {
                _depots[depotIndex, slotIndex] = null;
            }
        }

        // Restore battlers from the saved depot slots.
        foreach (DepotSlotSaveData slot in saveData.DepotSlots)
        {
            _depots[slot.DepotIndex, slot.SlotIndex] = new Battler(slot.BattlerData);
        }
    }
}

/// <summary>
/// Save data container for the battler storage.
/// </summary>
[System.Serializable]
public class DepotSaveData
{
    public List<DepotSlotSaveData> DepotSlots;
}

/// <summary>
/// Save data for a single depot slot.
/// </summary>
[System.Serializable]
public class DepotSlotSaveData
{
    public BattlerSaveData BattlerData;
    public int DepotIndex;
    public int SlotIndex;
}