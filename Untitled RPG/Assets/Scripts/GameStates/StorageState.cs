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
            StartCoroutine(HandleBattlerSelection(slotIndex));
        }
        else
        {
            if (_selectedBattlerToMove.IsMaster && !_storageUI.IsPartySlot(slotIndex))
            {
                _storageUI.PlaceBattlerIntoSlot(_selectedSlotToMove, _selectedBattlerToMove);
                StartCoroutine(HandlePlayerMoveAttempt());
                return;
            }

            if (slotIndex == _selectedSlotToMove)
            {
                _isMovingBattler = false;
                _storageUI.PlaceBattlerIntoSlot(slotIndex, _selectedBattlerToMove);
                _storageUI.SetStorageData();
                _storageUI.SetPartyData();
                AudioManager.Instance.PlaySFX(AudioID.UISelect);
                return;
            }

            _isMovingBattler = false;
            int firstSlotIndex = _selectedSlotToMove;
            int secondSlotIndex = slotIndex;
            Battler secondBattler = _storageUI.TakeBattlerFromSlot(secondSlotIndex);

            if (secondBattler == null && _storageUI.IsPartySlot(firstSlotIndex) && _storageUI.IsPartySlot(secondSlotIndex))
            {
                int partyIndex = firstSlotIndex / _storageUI.TotalColumns;
                _party.Battlers.RemoveAt(partyIndex);
                _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, _selectedBattlerToMove);
                _party.PartyUpdated();
                _storageUI.SetStorageData();
                _storageUI.SetPartyData();
                AudioManager.Instance.PlaySFX(AudioID.UISelect);
                return;
            }

            if (secondBattler != null && secondBattler.IsMaster && !_storageUI.IsPartySlot(firstSlotIndex))
            {
                _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, secondBattler);
                _storageUI.PlaceBattlerIntoSlot(firstSlotIndex, _selectedBattlerToMove);
                StartCoroutine(HandlePlayerMoveAttempt());
                return;
            }

            _storageUI.PlaceBattlerIntoSlot(secondSlotIndex, _selectedBattlerToMove);
            if (secondBattler != null)
            {
                _storageUI.PlaceBattlerIntoSlot(firstSlotIndex, secondBattler);
            }
            _party.Battlers.RemoveAll(static m => m == null);
            _party.PartyUpdated();
            _storageUI.SetStorageData();
            _storageUI.SetPartyData();
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
        }
    }

    private IEnumerator HandlePlayerMoveAttempt()
    {
        _isMovingBattler = false;
        _storageUI.RestoreSelection();
        _storageUI.SetStorageData();
        _storageUI.SetPartyData();
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        yield return DialogueManager.Instance.ShowDialogueText($"{PlayerController.Instance.Name} cannot be moved to storage.");
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
            case 0:
                Battler removedBattler = _storageUI.TakeBattlerFromSlot(slotIndex);
                if (removedBattler != null)
                {
                    _isMovingBattler = true;
                    _selectedSlotToMove = slotIndex;
                    _selectedBattlerToMove = removedBattler;
                    _storageUI.SaveSelection();
                }
                break;
            case 1:
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
            _storageUI.SetStorageData();
            _storageUI.SetPartyData();
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        }
        else
        {
            _storageUI.ResetSelection();
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _gameController.StateMachine.Pop();
        }
    }
}
