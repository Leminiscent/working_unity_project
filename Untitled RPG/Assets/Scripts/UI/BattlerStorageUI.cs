using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util.GenericSelectionUI;

public class BattlerStorageUI : SelectionUI<ImageSlot>
{
    [SerializeField] private List<ImageSlot> _storageSlots;
    [SerializeField] private TextMeshProUGUI _depotNameText;
    [SerializeField] private Image _transferImage;

    private const int TOTAL_COLUMNS = 9;
    private const float TRANSFER_IMAGE_VERTICAL_OFFSET = 50f;

    private List<StoragePartySlotUI> _partySlots = new();
    private List<StorageSlotUI> _depotSlots = new();
    private List<Image> _storageSlotImages = new();
    private BattleParty _party;
    private BattlerStorage _storage;

    public int SelectedDepot { get; private set; } = 0;
    public int TotalColumns => TOTAL_COLUMNS;

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
        SetSelectionSettings(SelectionType.Grid, TOTAL_COLUMNS);
    }

    public void SetStorageData()
    {
        for (int i = 0; i < _depotSlots.Count; i++)
        {
            Battler battler = _storage.GetBattler(SelectedDepot, i);

            if (battler != null)
            {
                _depotSlots[i].SetData(battler);
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
        if (HandleDepotInput())
        {
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
            Vector3 startPosition = _transferImage.transform.position;
            Vector3 endPosition = _storageSlotImages[_selectedItem].transform.position + (Vector3.up * TRANSFER_IMAGE_VERTICAL_OFFSET);
            _ = StartCoroutine(ObjectUtil.TweenPosition(_transferImage.gameObject, startPosition, endPosition));
        }
    }

    public bool IsPartySlot(int slotIndex)
    {
        return slotIndex % TOTAL_COLUMNS == 0;
    }

    // Helper to calculate the party index from a given slot index.
    private int GetPartySlotIndex(int slotIndex)
    {
        return slotIndex / TOTAL_COLUMNS;
    }

    // Helper to calculate the depot slot index from a given slot index.
    private int GetDepotSlotIndex(int slotIndex)
    {
        return slotIndex - ((slotIndex / TOTAL_COLUMNS) + 1);
    }

    // Handles input for depot paging and returns whether a change occurred.
    private bool HandleDepotInput()
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

        if (Input.GetButtonDown("Back"))
        {
            SelectedDepot = 0;
        }

        return prevSelectedDepot != SelectedDepot || Input.GetButtonDown("Back");
    }

    // Displays the transfer image at the specified slot index.
    private void ShowTransferImage(int slotIndex)
    {
        _transferImage.sprite = _storageSlotImages[slotIndex].sprite;
        _storageSlotImages[slotIndex].color = new Color(1, 1, 1, 0);

        Vector3 startPosition = _storageSlotImages[slotIndex].transform.position;
        Vector3 endPosition = startPosition + (Vector3.up * TRANSFER_IMAGE_VERTICAL_OFFSET);
        _ = StartCoroutine(ObjectUtil.ShiftOutwards(_transferImage.gameObject, startPosition, endPosition));
    }

    public Battler PeekBattlerInSlot(int slotIndex)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = GetPartySlotIndex(slotIndex);

            return partyIndex >= _party.Battlers.Count ? null : _party.Battlers[partyIndex];
        }
        else
        {
            int depotSlotIndex = GetDepotSlotIndex(slotIndex);
            return _storage.GetBattler(SelectedDepot, depotSlotIndex);
        }
    }

    public int GetSlotIndexForBattler(Battler battler)
    {
        int totalSlots = GetItemsCount();
        for (int i = 0; i < totalSlots; i++)
        {
            if (PeekBattlerInSlot(i) == battler)
            {
                return i;
            }
        }
        return -1;
    }

    public Battler TakeBattlerFromSlot(int slotIndex, bool isSwap = false)
    {
        Battler battler;
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = GetPartySlotIndex(slotIndex);

            if (partyIndex >= _party.Battlers.Count)
            {
                return null;
            }

            battler = _party.Battlers[partyIndex];
            if (battler == null)
            {
                return null;
            }

            _party.Battlers[partyIndex] = null;
        }
        else
        {
            int depotSlotIndex = GetDepotSlotIndex(slotIndex);
            battler = _storage.GetBattler(SelectedDepot, depotSlotIndex);
            if (battler == null)
            {
                return null;
            }
            _storage.RemoveBattler(SelectedDepot, depotSlotIndex);
        }

        if (!isSwap)
        {
            ShowTransferImage(slotIndex);
        }
        
        return battler;
    }

    public void PlaceBattlerIntoSlot(int slotIndex, Battler battler)
    {
        if (IsPartySlot(slotIndex))
        {
            int partyIndex = GetPartySlotIndex(slotIndex);
            if (partyIndex >= _party.Battlers.Count)
            {
                _party.Battlers.Add(battler);
            }
            else
            {
                _party.Battlers[partyIndex] = battler;
            }
        }
        else
        {
            int depotSlotIndex = GetDepotSlotIndex(slotIndex);
            _storage.AddBattler(battler, SelectedDepot, depotSlotIndex);
        }

        _transferImage.gameObject.SetActive(false);
    }

    public List<Battler> GetAllBattlers()
    {
        List<Battler> battlers = new();
        for (int i = 0; i < _storageSlots.Count; i++)
        {
            Battler battler = PeekBattlerInSlot(i);
            if (battler != null)
            {
                battlers.Add(battler);
            }
        }
        return battlers;
    }
}