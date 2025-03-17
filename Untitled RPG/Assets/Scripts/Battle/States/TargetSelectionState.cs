using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

/// <summary>
/// Manages target selection in battle by allowing the player to cycle through available targets
/// using vertical input, and confirms selection via action/back input.
/// </summary>
public class TargetSelectionState : State<BattleSystem>
{
    private BattleSystem _battleSystem;
    private int _selectedTarget = 0;
    private float _selectionTimer = 0;
    private const float SELECTION_SPEED = 5f;
    private const float INPUT_THRESHOLD = 0.2f;

    public static TargetSelectionState Instance { get; private set; }
    public bool SelectionMade { get; private set; } // Whether the player has confirmed their selection
    public int SelectedTarget => _selectedTarget; // The index of the selected target
    public bool IsTargetingAllies { get; set; } // Whether the player is targeting allies or enemies

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

    /// <summary>
    /// Updates the UI to visually indicate which unit is currently targeted.
    /// </summary>
    private void UpdateSelectionInUI()
    {
        // Cache the list of targets based on IsTargetingAllies flag.
        List<BattleUnit> targets = IsTargetingAllies ? _battleSystem.PlayerUnits : _battleSystem.EnemyUnits;
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].SetTargeted(i == _selectedTarget);
        }
    }

    /// <summary>
    /// Decrements the selection timer to regulate input frequency.
    /// </summary>
    private void UpdateSelectionTimer()
    {
        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }
    }

    /// <summary>
    /// Initializes the target selection state by resetting the selection and updating the UI.
    /// </summary>
    /// <param name="owner">The owning BattleSystem.</param>
    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;

        // Reset selection state.
        SelectionMade = false;
        _selectedTarget = 0;
        UpdateSelectionInUI();
    }

    /// <summary>
    /// Processes player input to update the target selection. Confirms selection on Action or cancels on Back.
    /// </summary>
    public override void Execute()
    {
        UpdateSelectionTimer();
        int previousSelection = _selectedTarget;
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Process vertical input if timer has elapsed.
        if (_selectionTimer == 0 && Mathf.Abs(verticalInput) > INPUT_THRESHOLD)
        {
            // Change selection based on the sign of input.
            _selectedTarget += -(int)Mathf.Sign(verticalInput);

            // Wrap around if selection goes out of bounds.
            int count = IsTargetingAllies ? _battleSystem.PlayerUnits.Count : _battleSystem.EnemyUnits.Count;
            if (_selectedTarget < 0)
            {
                _selectedTarget = count - 1;
            }
            else if (_selectedTarget >= count)
            {
                _selectedTarget = 0;
            }
            _selectionTimer = 1f / SELECTION_SPEED;
        }

        // If the selection changed, update the UI and play a sound.
        if (_selectedTarget != previousSelection)
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

    /// <summary>
    /// Clears target indicators when exiting the target selection state.
    /// </summary>
    public override void Exit()
    {
        List<BattleUnit> targets = IsTargetingAllies ? _battleSystem.PlayerUnits : _battleSystem.EnemyUnits;

        if (_selectedTarget < targets.Count) // Safety check in case the list is empty.
        {
            targets[_selectedTarget].SetTargeted(false);
        }
    }
}