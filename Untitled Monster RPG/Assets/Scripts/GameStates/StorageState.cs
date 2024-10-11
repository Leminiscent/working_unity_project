using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class StorageState : State<GameController>
{
    [SerializeField] MonsterStorageUI storageUI;
    bool isMovingMonster = false;
    int selectedSlotToMove = 0;
    Monster selectedMonsterToMove;
    GameController gameController;
    MonsterParty party;

    public static StorageState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        party = MonsterParty.GetPlayerParty();
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        storageUI.gameObject.SetActive(true);
        storageUI.SetPartyData();
        storageUI.SetStorageData();
        storageUI.OnSelected += OnSlotSelected;
        storageUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        storageUI.HandleUpdate();
    }

    public override void Exit()
    {
        storageUI.gameObject.SetActive(false);
        storageUI.OnSelected -= OnSlotSelected;
        storageUI.OnBack -= OnBack;
    }

    void OnSlotSelected(int slotIndex)
    {
        if (!isMovingMonster)
        {
            var monster = storageUI.TakeMonsterFromSlot(slotIndex);

            if (monster != null)
            {
                isMovingMonster = true;
                selectedSlotToMove = slotIndex;
                selectedMonsterToMove = monster;
            }
        }
        else
        {
            isMovingMonster = false;

            int firstSlotIndex = selectedSlotToMove;
            int secondSlotIndex = slotIndex;
            var secondMonster = storageUI.TakeMonsterFromSlot(secondSlotIndex);

            if (secondMonster == null && storageUI.IsPartySlot(firstSlotIndex) && storageUI.IsPartySlot(secondSlotIndex))
            {
                storageUI.PlaceMonsterIntoSlot(selectedSlotToMove, selectedMonsterToMove);
                storageUI.SetStorageData();
                storageUI.SetPartyData();
                return;
            }

            storageUI.PlaceMonsterIntoSlot(secondSlotIndex, selectedMonsterToMove);
            if (secondMonster != null)
            {
                storageUI.PlaceMonsterIntoSlot(firstSlotIndex, secondMonster);
            }
            party.Monsters.RemoveAll(m => m == null);
            party.PartyUpdated();
            storageUI.SetStorageData();
            storageUI.SetPartyData();
        }
    }

    void OnBack()
    {
        if (isMovingMonster)
        {
            isMovingMonster = false;
            storageUI.PlaceMonsterIntoSlot(selectedSlotToMove, selectedMonsterToMove);
            storageUI.SetStorageData();
            storageUI.SetPartyData();
        }
        else
        {
            gameController.StateMachine.Pop();
        }
    }
}
