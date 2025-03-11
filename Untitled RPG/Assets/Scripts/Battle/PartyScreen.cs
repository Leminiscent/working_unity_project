using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] private TextMeshProUGUI _messageText;

    private PartyMemberUI[] _memberSlots;
    private List<Battler> _battlers;
    private BattleParty _party;

    public Battler SelectedMember => _battlers[_selectedItem];

    public void Init()
    {
        _memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        SetSelectionSettings(SelectionType.Grid, 2);
        _party = BattleParty.GetPlayerParty();
        SetPartyData();
        _party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        _battlers = _party.Battlers;
        ClearItems();

        for (int i = 0; i < _memberSlots.Length; ++i)
        {
            if (i < _battlers.Count)
            {
                _memberSlots[i].gameObject.SetActive(true);
                _memberSlots[i].Init(_battlers[i]);
            }
            else
            {
                _memberSlots[i].gameObject.SetActive(false);
            }
        }

        IEnumerable<TextSlot> textSlots = _memberSlots.Select(static m => m.GetComponent<TextSlot>());
        SetItems(textSlots.Take(_battlers.Count).ToList());

        bool deputyAssigned = false;
        for (int i = 0; i < _battlers.Count; i++)
        {
            string roleMessage = "";
            if (_battlers[i].IsMaster)
            {
                roleMessage = "Master";
            }
            else if (!deputyAssigned)
            {
                roleMessage = "Deputy";
                deputyAssigned = true;
            }
            _memberSlots[i].SetMessage(roleMessage);
        }

        _messageText.text = "Choose a party member!";
    }

    public void ShowSkillBookUsability(SkillBook skillBook)
    {
        for (int i = 0; i < _battlers.Count; ++i)
        {
            string message = skillBook.CanBeLearned(_battlers[i]) ? "Learnable" : "Not Learnable";
            _memberSlots[i].SetMessage(message);
        }
    }

    public void SetMessageText(string message)
    {
        _messageText.text = message;
    }

    public void ClearMessageText()
    {
        for (int i = 0; i < _battlers.Count; ++i)
        {
            _memberSlots[i].SetMessage("");
        }
    }
}
