using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class MonsterStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] private List<ImageSlot> storageSlots;
    [SerializeField] private TextMeshProUGUI depotNameText;
    [SerializeField] private Image transferImage;
    private List<StoragePartySlotUI> partySlots = new();
    private List<StorageSlotUI> depotSlots = new();
    private List<Image> storageSlotImages = new();
    private MonsterParty party;
    private MonsterStorage storage;
    private int totalColumns = 9;

    public int SelectedDepot { get; private set; } = 0;

    private void Awake()
    {
        foreach (ImageSlot slot in storageSlots)
        {
            StorageSlotUI storageSlot = slot.GetComponent<StorageSlotUI>();

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
        storageSlotImages = storageSlots.Select(static s => s.transform.GetChild(0).GetComponent<Image>()).ToList();
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
            Monster monster = storage.GetMonster(SelectedDepot, i);

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

    public override void HandleUpdate()
    {
        int prevSelectedDepot = SelectedDepot;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectedDepot = SelectedDepot > 0 ? SelectedDepot - 1 : storage.NumberOfDepots - 1;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SelectedDepot = (SelectedDepot + 1) % storage.NumberOfDepots;
        }

        if (prevSelectedDepot != SelectedDepot || Input.GetButtonDown("Back"))
        {
            if (Input.GetButtonDown("Back"))
            {
                SelectedDepot = 0;
            }

            SetStorageData();
            UpdateSelectionInUI();
        }

        base.HandleUpdate();
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        depotNameText.text = "Depot " + (SelectedDepot + 1);

        if (transferImage.gameObject.activeSelf)
        {
            transferImage.transform.position = storageSlotImages[selectedItem].transform.position + (Vector3.up * 50f);
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
            if (monster == null)
            {
                return null;
            }

            party.Monsters[partyIndex] = null;
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / totalColumns) + 1);

            monster = storage.GetMonster(SelectedDepot, depotSlotIndex);
            if (monster == null)
            {
                return null;
            }

            storage.RemoveMonster(SelectedDepot, depotSlotIndex);
        }

        transferImage.sprite = storageSlotImages[slotIndex].sprite;
        transferImage.transform.position = storageSlotImages[slotIndex].transform.position + (Vector3.up * 50f);
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
            int depotSlotIndex = slotIndex - ((slotIndex / totalColumns) + 1);

            storage.AddMonster(monster, SelectedDepot, depotSlotIndex);

        }

        transferImage.gameObject.SetActive(false);
    }
}
