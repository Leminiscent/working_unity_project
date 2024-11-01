using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public class MonsterStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] private List<ImageSlot> _storageSlots;
    [SerializeField] private TextMeshProUGUI _depotNameText;
    [SerializeField] private Image _transferImage;

    private List<StoragePartySlotUI> _partySlots = new();
    private List<StorageSlotUI> _depotSlots = new();
    private List<Image> _storageSlotImages = new();
    private MonsterParty _party;
    private MonsterStorage _storage;
    private int _totalColumns = 9;

    public int SelectedDepot { get; private set; } = 0;

    private void Awake()
    {
        foreach (ImageSlot slot in _storageSlots)
        {
            if (slot.TryGetComponent(out StorageSlotUI storageSlot))
            {
                _depotSlots.Add(storageSlot);
            }
            else
            {
                _partySlots.Add(slot.GetComponent<StoragePartySlotUI>());
            }
        }

        _party = MonsterParty.GetPlayerParty();
        _storage = MonsterStorage.GetPlayerStorage();
        _storageSlotImages = _storageSlots.Select(static s => s.transform.GetChild(0).GetComponent<Image>()).ToList();
        _transferImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetItems(_storageSlots);
        SetSelectionSettings(SelectionType.Grid, _totalColumns);
    }

    public void SetStorageData()
    {
        for (int i = 0; i < _depotSlots.Count; i++)
        {
            Monster monster = _storage.GetMonster(SelectedDepot, i);

            if (monster != null)
            {
                _depotSlots[i].SetData(monster);
            }
            else
            {
                _depotSlots[i].ClearData();
            }
        }
    }

    public void SetPartyData()
    {
        for (int i = 0; i < _partySlots.Count; i++)
        {
            if (i < _party.Monsters.Count)
            {
                _partySlots[i].SetData(_party.Monsters[i]);
            }
            else
            {
                _partySlots[i].ClearData();
            }
        }
    }

    public override void HandleUpdate()
    {
        int prevSelectedDepot = SelectedDepot;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SelectedDepot = SelectedDepot > 0 ? SelectedDepot - 1 : _storage.NumberOfDepots - 1;
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SelectedDepot = (SelectedDepot + 1) % _storage.NumberOfDepots;
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

        _depotNameText.text = "Depot " + (SelectedDepot + 1);

        if (_transferImage.gameObject.activeSelf)
        {
            _transferImage.transform.position = _storageSlotImages[_selectedItem].transform.position + (Vector3.up * 50f);
        }
    }

    public bool IsPartySlot(int slotIndex)
    {
        return slotIndex % _totalColumns == 0;
    }

    public Monster TakeMonsterFromSlot(int slotIndex)
    {
        Monster monster;

        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / _totalColumns;

            if (partyIndex >= _party.Monsters.Count)
            {
                return null;
            }

            monster = _party.Monsters[partyIndex];
            if (monster == null)
            {
                return null;
            }

            _party.Monsters[partyIndex] = null;
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / _totalColumns) + 1);

            monster = _storage.GetMonster(SelectedDepot, depotSlotIndex);
            if (monster == null)
            {
                return null;
            }

            _storage.RemoveMonster(SelectedDepot, depotSlotIndex);
        }

        _transferImage.sprite = _storageSlotImages[slotIndex].sprite;
        _transferImage.transform.position = _storageSlotImages[slotIndex].transform.position + (Vector3.up * 50f);
        _storageSlotImages[slotIndex].color = new Color(1, 1, 1, 0);
        _transferImage.gameObject.SetActive(true);

        return monster;
    }

    public void PlaceMonsterIntoSlot(int slotIndex, Monster monster)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / _totalColumns;

            if (partyIndex >= _party.Monsters.Count)
            {
                _party.Monsters.Add(monster);
            }
            else
            {
                _party.Monsters[partyIndex] = monster;
            }
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / _totalColumns) + 1);

            _storage.AddMonster(monster, SelectedDepot, depotSlotIndex);
        }

        _transferImage.gameObject.SetActive(false);
    }
}
