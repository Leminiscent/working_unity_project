using System.Collections;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] private InventoryUI inventoryUI;
    private GameController gameController;
    private Inventory inventory;

    public ItemBase SelectedItem { get; private set; }
    public static InventoryState Instance { get; private set; }

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

    private void Start()
    {
        inventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        inventoryUI.gameObject.SetActive(true);
        SelectedItem = null;
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

    private void OnItemSelected(int selection)
    {
        SelectedItem = inventoryUI.SelectedItem;
        if (gameController.StateMachine.GetPrevState() != ShopSellingState.Instance)
        {
            StartCoroutine(SelectMonsterAndUseItem());
        }
        else
        {
            gameController.StateMachine.Pop();
        }
    }

    private void OnBack()
    {
        SelectedItem = null;
        gameController.StateMachine.Pop();
    }

    private IEnumerator SelectMonsterAndUseItem()
    {
        State<GameController> prevState = gameController.StateMachine.GetPrevState();

        if (!SelectedItem.DirectlyUsable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("This item can't be used directly!");
            yield break;
        }
        else if (prevState == BattleState.Instance)
        {
            if (!SelectedItem.UsableInBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used in battle!");
                yield break;
            }
        }
        else
        {
            if (!SelectedItem.UsableOutsideBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used outside of battle!");
                yield break;
            }
        }

        yield return gameController.StateMachine.PushAndWait(PartyState.Instance);

        if (prevState == BattleState.Instance)
        {
            if (UseItemState.Instance.ItemUsed)
            {
                gameController.StateMachine.Pop();
            }
        }
    }
}