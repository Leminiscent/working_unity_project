using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class LootSummaryState : State<BattleSystem>
{
    [SerializeField] GameObject lootSummaryUI;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] Transform lootListContainer;
    [SerializeField] GameObject lootItemPrefab;

    private int goldAmount;
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
        lootSummaryUI.SetActive(true);
        goldAmount = CalculateGold();
        items = CalculateLoot();
        DisplayGold();
        DisplayItems();
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            battleSystem.StateMachine.Pop();
        }
    }

    public int CalculateGold()
    {
        var gpDropped = battleSystem.EnemyUnit.Monster.Base.DropTable.GpDropped;

        return Random.Range(gpDropped.x, gpDropped.y + 1);
    }

    public Dictionary<ItemBase, int> CalculateLoot()
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

    private void DisplayGold()
    {
        goldText.text = $"{goldAmount} GP";
    }

    private void DisplayItems()
    {
        foreach (Transform child in lootListContainer)
            Destroy(child.gameObject);

        foreach (var item in items)
        {
            var lootItem = Instantiate(lootItemPrefab, lootListContainer);
            var itemText = lootItem.GetComponentInChildren<TextMeshProUGUI>();
            var itemIcon = lootItem.GetComponentInChildren<Image>();

            itemText.text = $"{item.Key.Name} x{item.Value}";
            itemIcon.sprite = item.Key.Icon;
        }
    }

    public override void Exit()
    {
        lootSummaryUI.SetActive(false);
    }
}
