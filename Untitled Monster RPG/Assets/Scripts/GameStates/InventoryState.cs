using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;
    GameController gameController;

    public static InventoryState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OnSelected += OnItemSelected;
        inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.OnSelected -= OnItemSelected;
        inventoryUI.OnBack -= OnBack;
    }

    void OnItemSelected(int selection)
    {
        gameController.StateMachine.Push(GamePartyState.Instance);
    }

    void OnBack()
    {
        gameController.StateMachine.Pop();
    }
}