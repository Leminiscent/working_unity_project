using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] private TextMeshProUGUI _messageText;

    private PartyMemberUI[] _memberSlots;
    private List<Monster> _monsters;
    private MonsterParty _party;

    public Monster SelectedMember => _monsters[selectedItem];

    public void Init()
    {
        _memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);
        _party = MonsterParty.GetPlayerParty();
        SetPartyData();
        _party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        _monsters = _party.Monsters;
        ClearItems();

        for (int i = 0; i < _memberSlots.Length; ++i)
        {
            if (i < _monsters.Count)
            {
                _memberSlots[i].gameObject.SetActive(true);
                _memberSlots[i].Init(_monsters[i]);
            }
            else
            {
                _memberSlots[i].gameObject.SetActive(false);
            }
        }

        IEnumerable<TextSlot> textSlots = _memberSlots.Select(static m => m.GetComponent<TextSlot>());

        SetItems(textSlots.Take(_monsters.Count).ToList());

        _messageText.text = "Choose a Monster!";
    }

    public void ShowSkillBookUsability(SkillBook skillBook)
    {
        for (int i = 0; i < _monsters.Count; ++i)
        {
            string message = skillBook.CanBeLearned(_monsters[i]) ? "Learnable" : "Not Learnable";

            _memberSlots[i].SetMessage(message);
        }
    }

    public void SetMessageText(string message)
    {
        _messageText.text = message;
    }

    public void ClearMessageText()
    {
        for (int i = 0; i < _monsters.Count; ++i)
        {
            _memberSlots[i].SetMessage("");
        }
    }
}
