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
        SetSelectionSettings(SelectionType.Grid, 9);
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
}
