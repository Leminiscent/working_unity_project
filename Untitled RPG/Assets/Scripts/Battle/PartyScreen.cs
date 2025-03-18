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

        if (_party == null)
        {
            Debug.LogError("PartyScreen.Init: Unable to retrieve the player party.");
            return;
        }

        SetPartyData();
        _party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        if (_party == null)
        {
            Debug.LogWarning("PartyScreen.SetPartyData: Party reference is null.");
            return;
        }

        _battlers = _party.Battlers;
        ClearItems();

        for (int i = 0; i < _memberSlots.Length; i++)
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
            if (_battlers[i].IsCommander)
            {
                roleMessage = "Commander";
            }
            else if (!deputyAssigned)
            {
                roleMessage = "Deputy";
                deputyAssigned = true;
            }
            _memberSlots[i].SetMessage(roleMessage);
        }

        if (_messageText != null)
        {
            _messageText.text = "Choose a party member!";
        }
    }

    public void UpdateBattleIndicators(BattleSystem battleSystem)
    {
        if (_battlers == null || _memberSlots == null)
        {
            return;
        }

        for (int i = 0; i < _battlers.Count; i++)
        {
            Battler battler = _battlers[i];
            bool isInBattle = battleSystem.PlayerUnits.Any(u => u.Battler == battler);
            bool isSwitching = battleSystem.UnableToSwitch(battler);

            if (isInBattle)
            {
                _memberSlots[i].SetMessage("In Battle");
            }
            else if (isSwitching)
            {
                _memberSlots[i].SetMessage("Preparing for battle");
            }
            else
            {
                _memberSlots[i].SetMessage("");
            }
        }
    }

    public void ShowSkillBookUsability(SkillBook skillBook)
    {
        if (_battlers == null || _memberSlots == null)
        {
            return;
        }

        for (int i = 0; i < _battlers.Count; i++)
        {
            string message = skillBook.CanBeLearned(_battlers[i]) ? "Learnable" : "Not Learnable";
            _memberSlots[i].SetMessage(message);
        }
    }

    public void SetMessageText(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    public void ClearMessageText()
    {
        if (_battlers == null || _memberSlots == null)
        {
            return;
        }

        for (int i = 0; i < _battlers.Count; i++)
        {
            _memberSlots[i].SetMessage("");
        }
    }
}