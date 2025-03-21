using System.Collections;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class UseItemState : State<GameController>
{
    [SerializeField] private PartyScreen _partyScreen;
    [SerializeField] private InventoryUI _inventoryUI;

    private GameController _gameController;
    private Inventory _inventory;

    public bool ItemUsed { get; private set; }
    public static UseItemState Instance { get; private set; }

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

        // Retrieve the player's inventory.
        _inventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        ItemUsed = false;
        _ = StartCoroutine(UseItem());
    }

    private IEnumerator UseItem()
    {
        // Retrieve the selected item and battler from the UI.
        ItemBase item = _inventoryUI.SelectedItem;
        Battler battler = _partyScreen.SelectedMember;

        if (item == null)
        {
            Debug.LogWarning("No item selected in InventoryUI.");
            _gameController.StateMachine.Pop();
            yield break;
        }
        if (battler == null)
        {
            Debug.LogWarning("No battler selected in PartyScreen.");
            _gameController.StateMachine.Pop();
            yield break;
        }

        // Handle SkillBooks separately.
        if (item is SkillBook)
        {
            yield return HandleSkillBooks();
        }
        else
        {
            // Handle TransformationItems.
            if (item is TransformationItem)
            {
                Transformation transformation = battler.CheckForTransformation(item);
                if (transformation != null)
                {
                    yield return TransformationState.Instance.PerformTransformation(battler, transformation);
                }
                else
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"The {item.Name} won't have any effect on {battler.Base.Name}!");
                    _gameController.StateMachine.Pop();
                    yield break;
                }
            }

            // Handle any other usable items.
            ItemBase usedItem = _inventory.UseItem(item, battler);
            if (usedItem != null)
            {
                ItemUsed = true;
                if (usedItem is RecoveryItem)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"The {usedItem.Name} was used on {battler.Base.Name}!");
                    yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} {usedItem.Message}!");
                }
            }
            else
            {
                // For RecoveryItems, display a message if the item has no effect.
                if (_inventoryUI.SelectedCategory == (int)ItemCategory.RecoveryItems)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"The {item.Name} won't have any effect on {battler.Base.Name}!");
                }
            }
        }

        // End the state.
        _gameController.StateMachine.Pop();
    }

    private IEnumerator HandleSkillBooks()
    {
        SkillBook skillBook = _inventoryUI.SelectedItem as SkillBook;
        if (skillBook == null)
        {
            yield break;
        }

        Battler battler = _partyScreen.SelectedMember;
        if (battler == null)
        {
            Debug.LogWarning("No battler selected in PartyScreen for SkillBook usage.");
            yield break;
        }

        // Check if the battler already knows the move.
        if (battler.HasMove(skillBook.Move))
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} already knows {skillBook.Move.Name}!");
            yield break;
        }

        // Check if the battler can learn the move.
        if (!skillBook.CanBeLearned(battler))
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} cannot learn {skillBook.Move.Name}!");
            yield break;
        }

        // If there is room to learn the new move directly.
        if (battler.Moves.Count < BattlerBase.MaxMoveCount)
        {
            battler.LearnMove(skillBook.Move);
            yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} has learned {skillBook.Move.Name}!");
        }
        else
        {
            // When the battler already knows the maximum number of moves, prompt for a move to forget.
            yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} is trying to learn {skillBook.Move.Name}!");
            yield return DialogueManager.Instance.ShowDialogueText($"But {battler.Base.Name} already knows {TextUtil.GetNumText(BattlerBase.MaxMoveCount)} moves!");
            yield return DialogueManager.Instance.ShowDialogueText($"Choose a move for {battler.Base.Name} to forget.", autoClose: false);

            // Set up the ForgettingMoveState.
            ForgettingMoveState.Instance.NewMove = skillBook.Move;
            ForgettingMoveState.Instance.CurrentMoves = battler.Moves.Select(static m => m.Base).ToList();

            yield return _gameController.StateMachine.PushAndWait(ForgettingMoveState.Instance);

            int moveIndex = ForgettingMoveState.Instance.Selection;

            // Check if the player canceled the move replacement.
            if (moveIndex == BattlerBase.MaxMoveCount || moveIndex == -1)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} did not learn {skillBook.Move.Name}!");
            }
            else
            {
                Move selectedMove = battler.Moves[moveIndex];
                yield return DialogueManager.Instance.ShowDialogueText($"{battler.Base.Name} forgot {selectedMove.Base.Name} and has learned {skillBook.Move.Name}!");
                battler.Moves[moveIndex] = new Move(skillBook.Move);
            }
        }
    }
}