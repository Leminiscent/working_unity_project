using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class TargetSelectionState : State<BattleSystem>
{
    private BattleSystem _battleSystem;
    private float _selectionTimer = 0;
    private const float SELECTION_SPEED = 5f;
    private const float INPUT_THRESHOLD = 0.2f;

    public static TargetSelectionState Instance { get; private set; }
    public bool SelectionMade { get; private set; }
    public int SelectedTarget { get; private set; } = 0;
    public bool IsTargetingAllies { get; set; }

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
    }

    private void UpdateSelectionInUI()
    {
        // Cache the list of targets based on IsTargetingAllies flag.
        List<BattleUnit> targets = IsTargetingAllies ? _battleSystem.PlayerUnits : _battleSystem.EnemyUnits;
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].SetTargeted(i == SelectedTarget);
        }
    }

    private void UpdateSelectionTimer()
    {
        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }
    }

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;

        // Reset selection state.
        SelectionMade = false;
        SelectedTarget = 0;
        UpdateSelectionInUI();
    }

    public override void Execute()
    {
        UpdateSelectionTimer();
        int previousSelection = SelectedTarget;
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Process vertical input if timer has elapsed.
        if (_selectionTimer == 0 && Mathf.Abs(verticalInput) > INPUT_THRESHOLD)
        {
            // Change selection based on the sign of input.
            SelectedTarget += -(int)Mathf.Sign(verticalInput);

            // Wrap around if selection goes out of bounds.
            int count = IsTargetingAllies ? _battleSystem.PlayerUnits.Count : _battleSystem.EnemyUnits.Count;
            if (SelectedTarget < 0)
            {
                SelectedTarget = count - 1;
            }
            else if (SelectedTarget >= count)
            {
                SelectedTarget = 0;
            }
            _selectionTimer = 1f / SELECTION_SPEED;
        }

        // If the selection changed, update the UI and play a sound.
        if (SelectedTarget != previousSelection)
        {
            UpdateSelectionInUI();
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
        }

        // Confirm selection.
        if (Input.GetButtonDown("Action"))
        {
            SelectionMade = true;
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
            _battleSystem.StateMachine.Pop();
        }
        // Cancel selection.
        else if (Input.GetButtonDown("Back"))
        {
            SelectionMade = false;
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _battleSystem.StateMachine.Pop();
        }
    }

    public override void Exit()
    {
        List<BattleUnit> targets = IsTargetingAllies ? _battleSystem.PlayerUnits : _battleSystem.EnemyUnits;

        if (SelectedTarget < targets.Count) // Safety check in case the list is empty.
        {
            targets[SelectedTarget].SetTargeted(false);
        }
    }
}