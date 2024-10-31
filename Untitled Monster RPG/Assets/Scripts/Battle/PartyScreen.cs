using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] private TextMeshProUGUI messageText;
    private PartyMemberUI[] memberSlots;
    private List<Monster> monsters;
    private MonsterParty party;

    public Monster SelectedMember => monsters[selectedItem];

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);
        party = MonsterParty.GetPlayerParty();
        SetPartyData();
        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        monsters = party.Monsters;
        ClearItems();

        for (int i = 0; i < memberSlots.Length; ++i)
        {
            if (i < monsters.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(monsters[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        IEnumerable<TextSlot> textSlots = memberSlots.Select(static m => m.GetComponent<TextSlot>());

        SetItems(textSlots.Take(monsters.Count).ToList());

        messageText.text = "Choose a Monster!";
    }

    public void ShowSkillBookUsability(SkillBook skillBook)
    {
        for (int i = 0; i < monsters.Count; ++i)
        {
            string message = skillBook.CanBeLearned(monsters[i]) ? "Learnable" : "Not Learnable";

            memberSlots[i].SetMessage(message);
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public void ClearMessageText()
    {
        for (int i = 0; i < monsters.Count; ++i)
        {
            memberSlots[i].SetMessage("");
        }
    }
}
