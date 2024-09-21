using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, ForgettingMove, Busy }

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    Action<ItemBase> OnItemUsed;
    int selectedItem = 0;
    int selectedCategory = 0;
    MoveBase moveToLearn;
    InventoryUIState state;
    const int itemsInViewport = 8;
    List<ItemSlotUI> slotUIList;
    Inventory inventory;
    RectTransform itemListRect;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.OnItemUsed = onItemUsed;

        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                ++selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                --selectedItem;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ++selectedCategory;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                --selectedCategory;
            }

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
            {
                selectedCategory = 0;
            }
            else if (selectedCategory < 0)
            {
                selectedCategory = Inventory.ItemCategories.Count - 1;
            }

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (prevCategory != selectedCategory)
            {
                ResetSelction();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                StartCoroutine(ItemSelected());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                onBack?.Invoke();
            }
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                ClosePartyScreen();
            };

            // partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
        else if (state == InventoryUIState.ForgettingMove)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelection(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if (GameController.Instance.State == GameState.Shop)
        {
            OnItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }

        if (GameController.Instance.State == GameState.Battle)
        {
            if (!item.UsableInBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item cannot be used in battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            if (!item.UsableOutsideBattle)
            {
                yield return DialogueManager.Instance.ShowDialogueText("This item cannot be used outside of battle!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        OpenPartyScreen();
        if (item is SkillBook)
        {
            partyScreen.ShowSkillBookUsability(item as SkillBook);
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;
        yield return HandleSkillBooks();

        var item = inventory.GetItem(selectedItem, selectedCategory);
        var monster = partyScreen.SelectedMember;

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
                ClosePartyScreen();
                yield break;
            }
        }

        var usedItem = inventory.UseItem(selectedItem, monster, selectedCategory);

        if (usedItem != null)
        {
            if (usedItem is RecoveryItem)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"The {usedItem.Name} was used on {monster.Base.Name}!");
                yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} {usedItem.Message}!");
            }
            OnItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.RecoveryItems)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"This item won't have any effect on {monster.Base.Name}!");
            }
        }

        ClosePartyScreen();
    }

    IEnumerator HandleSkillBooks()
    {
        var skillBook = inventory.GetItem(selectedItem, selectedCategory) as SkillBook;

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
            yield return ChooseMoveToForget(monster, skillBook.Move);
            yield return new WaitUntil(() => state != InventoryUIState.ForgettingMove);
        }
    }

    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogueManager.Instance.ShowDialogueText($"Choose a move for {monster.Base.Name} to forget.", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = InventoryUIState.ForgettingMove;
    }

    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
            {
                slotUIList[i].NameText.color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.Instance.InactiveColor;
            }
        }

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();
    }

    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        int maxScrollIndex = slotUIList.Count - itemsInViewport;
        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, maxScrollIndex) * slotUIList[0].Height;
        bool showUpArrow = selectedItem > itemsInViewport / 2;
        bool showDownArrow = selectedItem < maxScrollIndex + itemsInViewport / 2;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    void ResetSelction()
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.ClearMessageText();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelection(int moveIndex)
    {
        var monster = partyScreen.SelectedMember;

        DialogueManager.Instance.CloseDialogue();
        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == MonsterBase.MaxMoveCount)
        {
            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} did not learn {moveToLearn.Name}!");
        }
        else
        {
            var selectedMove = monster.Moves[moveIndex];

            yield return DialogueManager.Instance.ShowDialogueText($"{monster.Base.Name} forgot {selectedMove.Base.Name} and learned {moveToLearn.Name}!");
            monster.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}