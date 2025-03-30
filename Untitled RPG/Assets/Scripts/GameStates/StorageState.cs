using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class StorageState : State<GameController>
{
    [SerializeField] private BattlerStorageUI _storageUI;

    private bool _isMovingBattler = false;
    private int _selectedSlotToMove = 0;
    private Battler _selectedBattlerToMove;
    private GameController _gameController;
    private BattleParty _party;

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

        _party = BattleParty.GetPlayerParty();
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
        if (!_isMovingBattler)
        {
            // No battler is currently being moved; handle battler selection from the chosen slot.
            _ = StartCoroutine(HandleBattlerSelection(slotIndex));
        }
        else
        {
            // A battler is currently selected to move.
            // Special case: if the battler is a Commander and the target slot is not in the party,
            // cancel the move and show a message.
            if (_selectedBattlerToMove.IsCommander && !_storageUI.IsPartySlot(slotIndex))
            {
                _storageUI.PlaceBattlerIntoSlot(_selectedSlotToMove, _selectedBattlerToMove);
                _ = StartCoroutine(HandlePlayerMoveAttempt());
                return;
            }

            // If the selected slot is the same as the one being moved, place the battler back in its original slot.
            if (slotIndex == _selectedSlotToMove)
            {
                _isMovingBattler = false;
                _storageUI.PlaceBattlerIntoSlot(slotIndex, _selectedBattlerToMove);
                RefreshUI(AudioID.UISelect);
                return;
            }

            // Otherwise, attempt to swap battlers.
            _isMovingBattler = false;
            int firstSlotIndex = _selectedSlotToMove;
            int secondSlotIndex = slotIndex;
            Battler secondBattler = _storageUI.TakeBattlerFromSlot(secondSlotIndex);

            // If the target slot is empty and both slots are party slots, move the battler to the end of the party.
            if (secondBattler == null && _storageUI.IsPartySlot(firstSlotIndex) && _storageUI.IsPartySlot(secondSlotIndex))
            {
                int partyIndex = firstSlotIndex / _storageUI.TotalColumns;
                _party.Battlers.RemoveAt(partyIndex);
                _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, _selectedBattlerToMove);
                _party.PartyUpdated();
                RefreshUI(AudioID.UISelect, true);
                return;
            }

            // Special case: if the target battler is a Commander and the original slot is not a party slot, cancel the move.
            if (secondBattler != null && secondBattler.IsCommander && !_storageUI.IsPartySlot(firstSlotIndex))
            {
                _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, secondBattler);
                _storageUI.PlaceBattlerIntoSlot(firstSlotIndex, _selectedBattlerToMove);
                _ = StartCoroutine(HandlePlayerMoveAttempt());
                return;
            }

            // Otherwise, swap the battlers between the two slots.
            _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, _selectedBattlerToMove);
            if (secondBattler != null)
            {
                _storageUI.PlaceBattlerIntoSlot(firstSlotIndex, secondBattler);
            }
            // Clean up any null entries in the party list.
            _ = _party.Battlers.RemoveAll(static b => b == null);
            _party.PartyUpdated();
            RefreshUI(AudioID.UISelect, true);
        }
    }

    private IEnumerator HandlePlayerMoveAttempt()
    {
        _isMovingBattler = false;
        _storageUI.RestoreSelection();
        RefreshUI(AudioID.UIReturn);
        yield return DialogueManager.Instance.ShowDialogueText($"{PlayerController.Instance.Name} cannot be moved to the barracks.");
    }

    private IEnumerator HandleBattlerSelection(int slotIndex)
    {
        Battler battler = _storageUI.PeekBattlerInSlot(slotIndex);
        if (battler == null)
        {
            yield break;
        }

        AudioManager.Instance.PlaySFX(AudioID.UISelect);

        DynamicMenuState.Instance.MenuItems = new List<string>
        {
            "Move",
            "Summary",
            "Back"
        };
        yield return _gameController.StateMachine.PushAndWait(DynamicMenuState.Instance);

        switch (DynamicMenuState.Instance.SelectedItem)
        {
            case 0: // Move option
                Battler removedBattler = _storageUI.TakeBattlerFromSlot(slotIndex);
                if (removedBattler != null)
                {
                    _isMovingBattler = true;
                    _selectedSlotToMove = slotIndex;
                    _selectedBattlerToMove = removedBattler;
                    _storageUI.SaveSelection();
                }
                break;

            case 1: // Summary option
                List<Battler> battlers = _storageUI.GetAllBattlers();
                SummaryState.Instance.BattlersList = battlers;
                int index = battlers.IndexOf(battler);
                SummaryState.Instance.SelectedBattlerIndex = index < 0 ? 0 : index;
                yield return _gameController.StateMachine.PushAndWait(SummaryState.Instance);

                Battler selectedBattler = battlers[SummaryState.Instance.SelectedBattlerIndex];
                int targetSlotIndex = _storageUI.GetSlotIndexForBattler(selectedBattler);
                if (targetSlotIndex != -1)
                {
                    _storageUI.SetSelectedIndex(targetSlotIndex);
                }
                SummaryState.Instance.BattlersList = null;
                break;

            default:
                break;
        }
    }

    private void OnBack()
    {
        if (_isMovingBattler)
        {
            _isMovingBattler = false;
            _storageUI.RestoreSelection();
            _storageUI.PlaceBattlerIntoSlot(_selectedSlotToMove, _selectedBattlerToMove);
            RefreshUI(AudioID.UIReturn);
        }
        else
        {
            _storageUI.ResetSelection();
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _gameController.StateMachine.Pop();
        }
    }

    private void RefreshUI(AudioID sfx, bool updateSelection = false)
    {
        _storageUI.SetStorageData();
        _storageUI.SetPartyData();
        if (updateSelection)
        {
            // If the selection is updated, set the selected index to the battler being moved.
            _storageUI.SetSelectedIndex(_storageUI.GetSlotIndexForBattler(_selectedBattlerToMove));
        }
        AudioManager.Instance.PlaySFX(sfx);
    }
}