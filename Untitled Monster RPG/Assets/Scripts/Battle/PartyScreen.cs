using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    PartyMemberUI[] memberSlots;
    List<Monster> monsters;
    int selection = 0;

    public Monster SelectedMember => monsters[selection];

    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Monster> monsters)
    {
        this.monsters = monsters;

        for (int i = 0; i < memberSlots.Length; ++i)
        {
            if (i < monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Monster!";
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++selection;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --selection;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && selection < monsters.Count - 2)
        {
            selection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && selection > 1)
        {
            selection -= 2;
        }

        selection = Mathf.Clamp(selection, 0, monsters.Count - 1);

        if (prevSelection != selection)
        {
            UpdateMemberSelection(selection);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < memberSlots.Length; ++i)
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
