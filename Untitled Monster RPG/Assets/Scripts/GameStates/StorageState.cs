using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class StorageState : State<GameController>
{
    [SerializeField] MonsterStorageUI storageUI;

    public static StorageState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        storageUI.gameObject.SetActive(true);
        storageUI.SetPartyData();
        storageUI.SetStorageData();
    }

    public override void Execute()
    {
        storageUI.HandleUpdate();
    }

    public override void Exit()
    {
        storageUI.gameObject.SetActive(false);
    }
}
