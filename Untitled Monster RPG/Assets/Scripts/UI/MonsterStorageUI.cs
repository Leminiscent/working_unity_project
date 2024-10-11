using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.GenericSelectionUI;

public class MonsterStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> storageUISlots;
    List<StoragePartySlotUI> partySlots = new List<StoragePartySlotUI>();
    List<StorageSlotUI> storageSlots = new List<StorageSlotUI>();
    MonsterParty party;
    MonsterStorage storage;
    int totalColumns = 9;

    public int SelectedDepot { get; private set; } = 0;

    private void Awake()
    {
        foreach (var slot in storageUISlots)
        {
            var storageSlot = slot.GetComponent<StorageSlotUI>();

            if (storageSlot != null)
            {
                storageSlots.Add(storageSlot);
            }
            else
            {
                partySlots.Add(slot.GetComponent<StoragePartySlotUI>());
            }
        }

        party = MonsterParty.GetPlayerParty();
        storage = MonsterStorage.GetPlayerStorage();
    }

    private void Start()
    {
        SetItems(storageUISlots);
        SetSelectionSettings(SelectionType.Grid, totalColumns);
    }

    public void SetStorageData()
    {
        for (int i = 0; i < storageSlots.Count; i++)
        {
            var monster = storage.GetMonster(SelectedDepot, i);

            if (monster != null)
            {
                storageSlots[i].SetData(monster);
            }
            else
            {
                storageSlots[i].ClearData();
            }
        }
    }

    public void SetPartyData()
    {
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < party.Monsters.Count)
            {
                partySlots[i].SetData(party.Monsters[i]);
            }
            else
            {
                partySlots[i].ClearData();
            }
        }
    }

    public bool IsPartySlot(int slotIndex)
    {
        return slotIndex % totalColumns == 0;
    }

    public Monster TakeMonsterFromSlot(int slotIndex)
    {
        Monster monster;

        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / totalColumns;

            if (partyIndex >= party.Monsters.Count)
            {
                return null;
            }

            monster = party.Monsters[partyIndex];
            party.Monsters[partyIndex] = null;
        }
        else
        {
            int depotSlotIndex = slotIndex - (slotIndex / totalColumns + 1);

            monster = storage.GetMonster(SelectedDepot, depotSlotIndex);
            storage.RemoveMonster(SelectedDepot, depotSlotIndex);
        }

        return monster;
    }

    public void PlaceMonsterIntoSlot(int slotIndex, Monster monster)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / totalColumns;

            if (partyIndex >= party.Monsters.Count)
            {
                party.Monsters.Add(monster);
            }
            else
            {
                party.Monsters[partyIndex] = monster;
            }
        }
        else
        {
            int depotSlotIndex = slotIndex - (slotIndex / totalColumns + 1);

            storage.AddMonster(monster, SelectedDepot, depotSlotIndex);

        }
    }
}
