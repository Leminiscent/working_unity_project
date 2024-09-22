using System.Collections;
using System.Collections.Generic;
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
            // yield return ChooseMoveToForget(monster, skillBook.Move);
            // yield return new WaitUntil(() => state != InventoryUIState.ForgettingMove);
        }
    }
}
