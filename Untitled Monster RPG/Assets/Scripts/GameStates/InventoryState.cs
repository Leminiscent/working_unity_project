using System.Collections;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] private InventoryUI _inventoryUI;

    private GameController _gameController;

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

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _inventoryUI.gameObject.SetActive(true);
        SelectedItem = null;
        _inventoryUI.OnSelected += OnItemSelected;
        _inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        _inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        _inventoryUI.gameObject.SetActive(false);
        _inventoryUI.OnSelected -= OnItemSelected;
        _inventoryUI.OnBack -= OnBack;
    }

    private void OnItemSelected(int selection)
    {
        SelectedItem = _inventoryUI.SelectedItem;
        if (_gameController.StateMachine.GetPrevState() != ShopSellingState.Instance)
        {
            StartCoroutine(SelectMonsterAndUseItem());
        }
        else
        {
            _gameController.StateMachine.Pop();
        }
    }

    private void OnBack()
    {
        SelectedItem = null;
        _gameController.StateMachine.Pop();
    }

    private IEnumerator SelectMonsterAndUseItem()
    {
        State<GameController> prevState = _gameController.StateMachine.GetPrevState();

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

        if (prevState != BattleState.Instance)
        {
            yield return _gameController.StateMachine.PushAndWait(PartyState.Instance);
        }
        else
        {
            _gameController.StateMachine.Pop();
        }

        // if (prevState == BattleState.Instance)
        // {
        //     if (UseItemState.Instance.ItemUsed)
        //     {
        //         _gameController.StateMachine.Pop();
        //     }
        // }
    }
}