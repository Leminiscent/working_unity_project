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
        // Clear existing choices by iterating in reverse order
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Create new choice items
        List<TextSlot> items = new();
        foreach (string choice in choices)
        {
            TextSlot textSlot = Instantiate(_choiceTextPrefab, transform);
            textSlot.SetText(choice);
            items.Add(textSlot);
        }

        // Configure selection settings
        SetSelectionSettings(SelectionType.List, 1);
        SetItems(items);

        gameObject.SetActive(true);

        bool choiceMade = false;
        int selectedIndex = -1;

        // Local event handlers for selection/back events
        void onSelectedHandler(int index)
        {
            selectedIndex = index;
            choiceMade = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioID.UISelect);
            }
        }
        void onBackHandler()
        {
            selectedIndex = items.Count - 1;
            choiceMade = true;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            }
        }

        OnSelected += onSelectedHandler;
        OnBack += onBackHandler;

        // Wait until a choice is made
        yield return new WaitUntil(() => choiceMade);

        // Unsubscribe the event handlers
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
