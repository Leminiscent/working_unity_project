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
        gold = CalculateGold();
        items = CalculateItems();
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

    public int CalculateGold()
    {
        var gpDropped = battleSystem.EnemyUnit.Monster.Base.DropTable.GpDropped;

        return Random.Range(gpDropped.x, gpDropped.y + 1);
    }

    public Dictionary<ItemBase, int> CalculateItems()
    {
        var itemDrops = battleSystem.EnemyUnit.Monster.Base.DropTable.ItemDrops;
        Dictionary<ItemBase, int> lootDict = new();

        foreach (var itemDrop in itemDrops)
        {
            if (Random.value <= itemDrop.DropChance)
            {
                int quantity = Random.Range(itemDrop.QuantityRange.x, itemDrop.QuantityRange.y + 1);

                lootDict.Add(itemDrop.Item, quantity);
            }
        }

        return lootDict;
    }

    public override void Exit()
    {
        lootSummaryUI.gameObject.SetActive(false);
    }
}
