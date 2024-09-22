using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GenericSelectionUI;

public enum InventoryUIState { ItemSelection, PartySelection, ForgettingMove, Busy }

public class InventoryUI : SelectionUI<TextSlot>
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

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());
        UpdateSelectionInUI();
    }

    public override void HandleUpdate()
    {
        int prevCategory = selectedCategory;

        if (Input.GetKeyDown(KeyCode.RightArrow))
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

        if (prevCategory != selectedCategory)
        {
            ResetSelction();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
            UpdateItemList();
        }

        base.HandleUpdate();
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

    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove)
    {
        state = InventoryUIState.Busy;
        yield return DialogueManager.Instance.ShowDialogueText($"Choose a move for {monster.Base.Name} to forget.", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;
        state = InventoryUIState.ForgettingMove;
    }

    public override void UpdateSelectionInUI()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;

            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }

        HandleScrolling();

        base.UpdateSelectionInUI();
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

    public ItemBase SelectedItem => inventory.GetItem(selectedItem, selectedCategory);

    public int SelectedCategory => selectedCategory;
}