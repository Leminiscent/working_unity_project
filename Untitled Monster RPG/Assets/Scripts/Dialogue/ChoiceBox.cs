using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.GenericSelectionUI;

public class ChoiceBox : SelectionUI<TextSlot>
{
    [SerializeField] private TextSlot _choiceTextPrefab;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        List<TextSlot> items = new();
        foreach (string choice in choices)
        {
            TextSlot textSlot = Instantiate(_choiceTextPrefab, transform);
            textSlot.SetText(choice);
            items.Add(textSlot);
        }

        SetSelectionSettings(SelectionType.List, 1);
        SetItems(items);

        gameObject.SetActive(true);

        bool choiceMade = false;
        int selectedIndex = -1;

        void onSelectedHandler(int index)
        {
            selectedIndex = index;
            choiceMade = true;
        }
        void onBackHandler()
        {
            selectedIndex = items.Count - 1;
            choiceMade = true;
        }

        OnSelected += onSelectedHandler;
        OnBack += onBackHandler;

        yield return new WaitUntil(() => choiceMade);

        OnSelected -= onSelectedHandler;
        OnBack -= onBackHandler;

        onChoiceSelected?.Invoke(selectedIndex);
        gameObject.SetActive(false);
        ResetSelection();
    }

    private void Update()
    {
        HandleUpdate();
    }
}
