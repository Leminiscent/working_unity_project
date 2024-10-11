using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class MonsterStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] List<ImageSlot> storageSlots;
    [SerializeField] Image transferImage;
    List<StoragePartySlotUI> partySlots = new List<StoragePartySlotUI>();
    List<StorageSlotUI> depotSlots = new List<StorageSlotUI>();
    List<Image> storageSlotImages = new List<Image>();
    MonsterParty party;
    MonsterStorage storage;
    int totalColumns = 9;

    public int SelectedDepot { get; private set; } = 0;

    private void Awake()
    {
        foreach (var slot in storageSlots)
        {
            var storageSlot = slot.GetComponent<StorageSlotUI>();

            if (storageSlot != null)
            {
                depotSlots.Add(storageSlot);
            }
            else
            {
                partySlots.Add(slot.GetComponent<StoragePartySlotUI>());
            }
        }

        party = MonsterParty.GetPlayerParty();
        storage = MonsterStorage.GetPlayerStorage();
        storageSlotImages = storageSlots.Select(s => s.transform.GetChild(0).GetComponent<Image>()).ToList();
        transferImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetItems(storageSlots);
        SetSelectionSettings(SelectionType.Grid, totalColumns);
    }

    public void SetStorageData()
    {
        for (int i = 0; i < depotSlots.Count; i++)
        {
            var monster = storage.GetMonster(SelectedDepot, i);

            if (monster != null)
            {
                depotSlots[i].SetData(monster);
            }
            else
            {
                depotSlots[i].ClearData();
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

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        if (transferImage.gameObject.activeSelf)
        {
            transferImage.transform.position = storageSlotImages[selectedItem].transform.position + Vector3.up * 50f;
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
        
        transferImage.sprite = storageSlotImages[slotIndex].sprite;
        transferImage.transform.position = storageSlotImages[slotIndex].transform.position + Vector3.up * 50f;
        storageSlotImages[slotIndex].color = new Color(1, 1, 1, 0);
        transferImage.gameObject.SetActive(true);

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

        transferImage.gameObject.SetActive(false);
    }
}
