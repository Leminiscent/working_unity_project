using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    GameController gameController;
    Inventory inventory;

    public static UseItemState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        inventory = Inventory.GetInventory();
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;

        StartCoroutine(UseItem());
    }

    IEnumerator UseItem()
    {
        var item = inventoryUI.SelectedItem;
        var monster = partyScreen.SelectedMember;

        if (item is SkillBook)
        {
            yield return HandleSkillBooks();
        }
        else
        {
            if (item is TransformationItem)
            {
                var transformation = monster.CheckForTransformation(item);

                if (transformation != null)
                {
                    yield return TransformationManager.Instance.Transform(monster, transformation);
                }
                else
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {monster.Base.Name}!");
                    gameController.StateMachine.Pop();
                    yield break;
                }
            }

            var usedItem = inventory.UseItem(item, monster);

            if (usedItem != null)
            {
                if (usedItem is RecoveryItem)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"The {usedItem.Name} was used on {monster.Base.Name}!");
                    yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} {usedItem.Message}!");
                }
            }
            else
            {
                if (inventoryUI.SelectedCategory == (int)ItemCategory.RecoveryItems)
                {
                    yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {monster.Base.Name}!");
                }
            }
        }

        gameController.StateMachine.Pop();
    }

    IEnumerator HandleSkillBooks()
    {
        var skillBook = inventoryUI.SelectedItem as SkillBook;

        if (skillBook == null)
        {
            yield break;
        }

        var monster = partyScreen.SelectedMember;

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
            ForgettingMoveState.Instance.CurrentMoves = monster.Moves.Select(m => m.Base).ToList();
            yield return gameController.StateMachine.PushAndWait(ForgettingMoveState.Instance);

            int moveIndex = ForgettingMoveState.Instance.Selection;

            if (moveIndex == MonsterBase.MaxMoveCount || moveIndex == -1)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} did not learn {skillBook.Move.Name}!");
            }
            else
            {
                var selectedMove = monster.Moves[moveIndex];

                yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} forgot {selectedMove.Base.Name} and learned {skillBook.Move.Name}!");
                monster.Moves[moveIndex] = new Move(skillBook.Move);
            }
        }
    }
}
