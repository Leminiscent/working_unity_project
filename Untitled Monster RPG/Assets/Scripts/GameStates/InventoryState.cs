using System.Collections;
using UnityEngine;
using Utils.StateMachine;

public class InventoryState : State<GameController>
{
    [SerializeField] private InventoryUI _inventoryUI;

    private GameController _gameController;
    private State<GameController> _prevState;

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
        _prevState = _gameController.StateMachine.GetPrevState();
        _inventoryUI.gameObject.SetActive(true);
        if (_prevState == BattleState.Instance)
        {
            _inventoryUI.HideMoneyText();
        }
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
        if (!SelectedItem.DirectlyUsable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("This item can't be used directly!");
            SelectedItem = null;
            yield break;
        }
        else if (_prevState == BattleState.Instance)
        {
            if (!SelectedItem.UsableInBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used in battle!");
                SelectedItem = null;
                yield break;
            }
        }
        else
        {
            if (!SelectedItem.UsableOutsideBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used outside of battle!");
                SelectedItem = null;
                yield break;
            }
        }

        if (_prevState != BattleState.Instance)
        {
            yield return _gameController.StateMachine.PushAndWait(PartyState.Instance);
        }
        else
        {
            _gameController.StateMachine.Pop();
        }
    }
}