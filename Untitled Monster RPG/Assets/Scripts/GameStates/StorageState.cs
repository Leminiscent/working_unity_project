using UnityEngine;
using Utils.StateMachine;

public class StorageState : State<GameController>
{
    [SerializeField] private MonsterStorageUI _storageUI;
    private bool _isMovingMonster = false;
    private int _selectedSlotToMove = 0;
    private Monster _selectedMonsterToMove;
    private GameController _gameController;
    private MonsterParty _party;

    public static StorageState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        _party = MonsterParty.GetPlayerParty();
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _storageUI.gameObject.SetActive(true);
        _storageUI.SetPartyData();
        _storageUI.SetStorageData();
        _storageUI.OnSelected += OnSlotSelected;
        _storageUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        _storageUI.HandleUpdate();
    }

    public override void Exit()
    {
        _storageUI.gameObject.SetActive(false);
        _storageUI.OnSelected -= OnSlotSelected;
        _storageUI.OnBack -= OnBack;
    }

    private void OnSlotSelected(int slotIndex)
    {
        if (!_isMovingMonster)
        {
            Monster monster = _storageUI.TakeMonsterFromSlot(slotIndex);

            if (monster != null)
            {
                _isMovingMonster = true;
                _selectedSlotToMove = slotIndex;
                _selectedMonsterToMove = monster;
            }
        }
        else
        {
            _isMovingMonster = false;

            int firstSlotIndex = _selectedSlotToMove;
            int secondSlotIndex = slotIndex;
            Monster secondMonster = _storageUI.TakeMonsterFromSlot(secondSlotIndex);

            if (secondMonster == null && _storageUI.IsPartySlot(firstSlotIndex) && _storageUI.IsPartySlot(secondSlotIndex))
            {
                _storageUI.PlaceMonsterIntoSlot(_selectedSlotToMove, _selectedMonsterToMove);
                _storageUI.SetStorageData();
                _storageUI.SetPartyData();
                return;
            }

            _storageUI.PlaceMonsterIntoSlot(secondSlotIndex, _selectedMonsterToMove);
            if (secondMonster != null)
            {
                _storageUI.PlaceMonsterIntoSlot(firstSlotIndex, secondMonster);
            }
            _party.Monsters.RemoveAll(static m => m == null);
            _party.PartyUpdated();
            _storageUI.SetStorageData();
            _storageUI.SetPartyData();
        }
    }

    private void OnBack()
    {
        if (_isMovingMonster)
        {
            _isMovingMonster = false;
            _storageUI.PlaceMonsterIntoSlot(_selectedSlotToMove, _selectedMonsterToMove);
            _storageUI.SetStorageData();
            _storageUI.SetPartyData();
        }
        else
        {
            _gameController.StateMachine.Pop();
        }
    }
}
