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
    private BattleParty _party;
    private BattlerStorage _storage;
    private int _totalColumns = 9;

    public int SelectedDepot { get; private set; } = 0;
    public int TotalColumns => _totalColumns;

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

        _party = BattleParty.GetPlayerParty();
        _storage = BattlerStorage.GetPlayerStorage();
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
            Battler monster = _storage.GetBattler(SelectedDepot, i);

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
            if (i < _party.Battlers.Count)
            {
                _partySlots[i].SetData(_party.Battlers[i]);
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

        if (Input.GetButtonDown("Page Left"))
        {
            SelectedDepot = SelectedDepot > 0 ? SelectedDepot - 1 : _storage.NumberOfDepots - 1;
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
        }
        else if (Input.GetButtonDown("Page Right"))
        {
            SelectedDepot = (SelectedDepot + 1) % _storage.NumberOfDepots;
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
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

    public Battler PeekMonsterInSlot(int slotIndex)
    {
        Battler monster;

        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / _totalColumns;

            if (partyIndex >= _party.Battlers.Count)
            {
                return null;
            }

            monster = _party.Battlers[partyIndex];
            return monster;
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / _totalColumns) + 1);

            monster = _storage.GetBattler(SelectedDepot, depotSlotIndex);
            return monster;
        }
    }

    public int GetSlotIndexForMonster(Battler monster)
    {
        int totalSlots = GetItemsCount();
        for (int i = 0; i < totalSlots; i++)
        {
            if (PeekMonsterInSlot(i) == monster)
            {
                return i;
            }
        }
        return -1;
    }

    public Battler TakeMonsterFromSlot(int slotIndex)
    {
        Battler monster;

        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / _totalColumns;

            if (partyIndex >= _party.Battlers.Count)
            {
                return null;
            }

            monster = _party.Battlers[partyIndex];
            if (monster == null)
            {
                return null;
            }

            _party.Battlers[partyIndex] = null;
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / _totalColumns) + 1);

            monster = _storage.GetBattler(SelectedDepot, depotSlotIndex);
            if (monster == null)
            {
                return null;
            }

            _storage.RemoveBattler(SelectedDepot, depotSlotIndex);
        }

        _transferImage.sprite = _storageSlotImages[slotIndex].sprite;
        _transferImage.transform.position = _storageSlotImages[slotIndex].transform.position + (Vector3.up * 50f);
        _storageSlotImages[slotIndex].color = new Color(1, 1, 1, 0);
        _transferImage.gameObject.SetActive(true);

        return monster;
    }

    public void PlaceMonsterIntoSlot(int slotIndex, Battler monster)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = slotIndex / _totalColumns;

            if (partyIndex >= _party.Battlers.Count)
            {
                _party.Battlers.Add(monster);
            }
            else
            {
                _party.Battlers[partyIndex] = monster;
            }
        }
        else
        {
            int depotSlotIndex = slotIndex - ((slotIndex / _totalColumns) + 1);

            _storage.AddBattler(monster, SelectedDepot, depotSlotIndex);
        }

        _transferImage.gameObject.SetActive(false);
    }

    public List<Battler> GetAllMonsters()
    {
        List<Battler> monsters = new();
        for (int i = 0; i < _storageSlots.Count; i++)
        {
            Battler monster = PeekMonsterInSlot(i);
            if (monster != null)
            {
                monsters.Add(monster);
            }
        }
        return monsters;
    }
}
