using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class TargetSelectionState : State<BattleSystem>
{
    private int _selectedTargetIndex = 0;
    private BattleSystem _battleSystem;

    public static TargetSelectionState Instance { get; private set; }

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

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _selectedTargetIndex++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _selectedTargetIndex--;
        }
    }
}