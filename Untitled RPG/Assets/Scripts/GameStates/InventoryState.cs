using System.Collections;
using UnityEngine;
using Util.StateMachine;

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

        if (_inventoryUI == null)
        {
            Debug.LogError("InventoryUI reference is missing.");
            return;
        }

        _inventoryUI.gameObject.SetActive(true);

        // Hide money text if coming from a battle state, otherwise show it.
        if (_prevState == BattleState.Instance)
        {
            _inventoryUI.HideMoneyText();
        }
        else
        {
            _inventoryUI.ShowMoneyText();
        }

        SelectedItem = null;
        _inventoryUI.EnableInput(true);
        _inventoryUI.OnSelected += OnItemSelected;
        _inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.HandleUpdate();
        }
    }

    public override void Exit()
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.ResetInventoryScreen();
            _inventoryUI.gameObject.SetActive(false);
            _inventoryUI.OnSelected -= OnItemSelected;
            _inventoryUI.OnBack -= OnBack;
        }
    }

    private void OnItemSelected(int selection)
    {
        SelectedItem = _inventoryUI.SelectedItem;
        AudioManager.Instance.PlaySFX(AudioID.UISelect);

        // If the previous state is not ShopSelling, then proceed to item usage.
        if (_gameController.StateMachine.GetPrevState() != ShopSellingState.Instance)
        {
            _ = StartCoroutine(SelectBattlerAndUseItem());
        }
        else
        {
            _gameController.StateMachine.Pop();
        }
    }

    private void OnBack()
    {
        SelectedItem = null;
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        StartCoroutine(LeaveState());
    }

    private IEnumerator LeaveState()
    {
        _inventoryUI.EnableInput(false);
        yield return Fader.Instance.FadeIn(0.5f);

        _gameController.StateMachine.ChangeState(CutsceneState.Instance);
        yield return Fader.Instance.FadeOut(0.5f);

        _gameController.StateMachine.Pop();
    }

    private IEnumerator SelectBattlerAndUseItem()
    {
        // Check if the item is directly usable.
        if (!SelectedItem.DirectlyUsable)
        {
            yield return DialogueManager.Instance.ShowDialogueText("This item can't be used directly!");
            SelectedItem = null;
            yield break;
        }
        // Check usability in battle.
        else if (_prevState == BattleState.Instance)
        {
            if (!SelectedItem.UsableInBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used in battle!");
                SelectedItem = null;
                yield break;
            }
        }
        // Check usability outside of battle.
        else
        {
            if (!SelectedItem.UsableOutsideBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item can't be used outside of battle!");
                SelectedItem = null;
                yield break;
            }
        }

        // If not coming from battle, push the PartyState to choose a battler for item use.
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