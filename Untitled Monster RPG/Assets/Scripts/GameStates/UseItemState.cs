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

        _inventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        ItemUsed = false;

        StartCoroutine(UseItem());
    }

    private IEnumerator UseItem()
    {
        ItemBase item = _inventoryUI.SelectedItem;
        Monster monster = _partyScreen.SelectedMember;

        if (item is SkillBook)
        {
            yield return HandleSkillBooks();
        }
        else
        {
            if (item is TransformationItem)
            {
                Transformation transformation = monster.CheckForTransformation(item);

                if (transformation != null)
                {
                    yield return TransformationState.Instance.Transform(monster, transformation);
                }
                else
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {monster.Base.Name}!");
                    _gameController.StateMachine.Pop();
                    yield break;
                }
            }

            ItemBase usedItem = _inventory.UseItem(item, monster);

            if (usedItem != null)
            {
                ItemUsed = true;
                if (usedItem is RecoveryItem)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"The {usedItem.Name} was used on {monster.Base.Name}!");
                    yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} {usedItem.Message}!");
                }
            }
            else
            {
                if (_inventoryUI.SelectedCategory == (int)ItemCategory.RecoveryItems)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {monster.Base.Name}!");
                }
            }
        }

        _gameController.StateMachine.Pop();
    }

    private IEnumerator HandleSkillBooks()
    {
        SkillBook skillBook = _inventoryUI.SelectedItem as SkillBook;

        if (skillBook == null)
        {
            yield break;
        }

        Monster monster = _partyScreen.SelectedMember;

        if (monster.HasMove(skillBook.Move))
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} already knows {skillBook.Move.Name}!");
            yield break;
        }

        if (!skillBook.CanBeLearned(monster))
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} cannot learn {skillBook.Move.Name}!");
            yield break;
        }

        if (monster.Moves.Count < MonsterBase.MaxMoveCount)
        {
            monster.LearnMove(skillBook.Move);
            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} learned {skillBook.Move.Name}!");
        }
        else
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} is trying to learn {skillBook.Move.Name}!");
            yield return DialogueManager.Instance.ShowDialogueText($"But {monster.Base.Name} already knows {MonsterBase.MaxMoveCount} moves!");
            yield return DialogueManager.Instance.ShowDialogueText($"Choose a move for {monster.Base.Name} to forget.", true, false);
            ForgettingMoveState.Instance.NewMove = skillBook.Move;
            ForgettingMoveState.Instance.CurrentMoves = monster.Moves.Select(static m => m.Base).ToList();
            yield return _gameController.StateMachine.PushAndWait(ForgettingMoveState.Instance);

            int moveIndex = ForgettingMoveState.Instance.Selection;

            if (moveIndex == MonsterBase.MaxMoveCount || moveIndex == -1)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} did not learn {skillBook.Move.Name}!");
            }
            else
            {
                Move selectedMove = monster.Moves[moveIndex];

                yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} forgot {selectedMove.Base.Name} and learned {skillBook.Move.Name}!");
                monster.Moves[moveIndex] = new Move(skillBook.Move);
            }
        }
    }
}
