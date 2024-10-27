using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class LootSummaryState : State<BattleSystem>
{
    [SerializeField] LootSummaryUI lootSummaryUI;
    private int gold;
    private Dictionary<ItemBase, int> items;
    BattleSystem battleSystem;

    public static LootSummaryState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(BattleSystem owner)
    {
        battleSystem = owner;
        lootSummaryUI.gameObject.SetActive(true);
        CalculateGold();
        CalculateItems();
        lootSummaryUI.DisplayGold(gold);
        lootSummaryUI.DisplayItems(items);
    }

    public override void Execute()
    {
        if (Input.GetButtonDown("Action") || Input.GetButtonDown("Back"))
        {
            battleSystem.StateMachine.Pop();
        }
    }

    public void CalculateGold()
    {
        var gpDropped = battleSystem.EnemyUnit.Monster.Base.DropTable.GpDropped;

        gold = Random.Range(gpDropped.x, gpDropped.y + 1);
    }

    public void CalculateItems()
    {
        var itemDrops = battleSystem.EnemyUnit.Monster.Base.DropTable.ItemDrops;

        items = new();

        foreach (var itemDrop in itemDrops)
        {
            if (Random.value <= itemDrop.DropChance)
            {
                int quantity = Random.Range(itemDrop.QuantityRange.x, itemDrop.QuantityRange.y + 1);

                items.Add(itemDrop.Item, quantity);
            }
        }
    }

    public override void Exit()
    {
        lootSummaryUI.gameObject.SetActive(false);
    }
}
