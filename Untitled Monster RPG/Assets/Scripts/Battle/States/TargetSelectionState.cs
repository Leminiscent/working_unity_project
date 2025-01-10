using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class TargetSelectionState : State<BattleSystem>
{
    private int _selectedTarget = 0;
    private BattleSystem _battleSystem;

    public static TargetSelectionState Instance { get; private set; }
    public bool SelectionMade { get; private set; }
    public int SelectedTarget => _selectedTarget;

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
        for (int i = 0; i < _battleSystem.EnemyUnits.Count; i++)
        {
            _battleSystem.EnemyUnits[i].SetSelected(i == _selectedTarget);
        }
    }

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        SelectionMade = false;
        _selectedTarget = 0;
        UpdateSelectionInUI();
    }

    public override void Execute()
    {
        int prevSelection = _selectedTarget;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _selectedTarget++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _selectedTarget--;
        }

        if (_selectedTarget != prevSelection)
        {
            UpdateSelectionInUI();
        }

        if (Input.GetButtonDown("Action"))
        {
            SelectionMade = true;
            _battleSystem.StateMachine.Pop();
        }
        else if (Input.GetButtonDown("Back"))
        {
            SelectionMade = false;
            _battleSystem.StateMachine.Pop();
        }
    }
}