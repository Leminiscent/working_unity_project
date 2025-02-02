using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(HandleMonsterSelection(slotIndex));
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

    private IEnumerator HandleMonsterSelection(int slotIndex)
    {
        Monster monster = _storageUI.PeekMonsterInSlot(slotIndex);
        if (monster == null)
        {
            yield break;
        }

        DynamicMenuState.Instance.MenuItems = new List<string>
        {
            "Move",
            "Summary",
            "Back"
        };
        yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);
        switch (DynamicMenuState.Instance.SelectedItem)
        {
            case 0:
                Monster removedMonster = _storageUI.TakeMonsterFromSlot(slotIndex);
                if (removedMonster != null)
                {
                    _isMovingMonster = true;
                    _selectedSlotToMove = slotIndex;
                    _selectedMonsterToMove = removedMonster;
                    _storageUI.SaveSelection();
                }
                break;
            case 1:
                List<Monster> monsters = _storageUI.GetAllMonsters();
                SummaryState.Instance.MonstersList = monsters;
                int index = monsters.IndexOf(monster);
                SummaryState.Instance.SelectedMonsterIndex = index < 0 ? 0 : index;
                yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);

                Monster selectedMonster = monsters[SummaryState.Instance.SelectedMonsterIndex];
                int targetSlotIndex = _storageUI.GetSlotIndexForMonster(selectedMonster);
                if (targetSlotIndex != -1)
                {
                    _storageUI.SetSelectedIndex(targetSlotIndex);
                }
                SummaryState.Instance.MonstersList = null;
                break;
            default:
                break;
        }
    }

    private void OnBack()
    {
        if (_isMovingMonster)
        {
            _isMovingMonster = false;
            _storageUI.RestoreSelection();
            _storageUI.PlaceMonsterIntoSlot(_selectedSlotToMove, _selectedMonsterToMove);
            _storageUI.SetStorageData();
            _storageUI.SetPartyData();
        }
        else
        {
            _storageUI.ResetSelection();
            _gameController.StateMachine.Pop();
        }
    }
}
