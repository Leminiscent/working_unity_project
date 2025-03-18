using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

/// <summary>
/// Manages the party screen UI, displaying party member data and handling selection and status messages.
/// </summary>
public class PartyScreen : SelectionUI<TextSlot>
{
    [SerializeField] private TextMeshProUGUI _messageText;

    private PartyMemberUI[] _memberSlots;
    private List<Battler> _battlers;
    private BattleParty _party;

    public Battler SelectedMember => _battlers[_selectedItem];

    /// <summary>
    /// Initializes the party screen UI by fetching party members and setting up event listeners.
    /// </summary>
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

    /// <summary>
    /// Updates the party data on the UI, including member details and role messages.
    /// </summary>
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

    /// <summary>
    /// Updates the battle indicators on each party member's UI based on their battle status.
    /// </summary>
    /// <param name="battleSystem">The current battle system instance.</param>
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

    /// <summary>
    /// Displays whether a skill book can be learned by each party member.
    /// </summary>
    /// <param name="skillBook">The skill book to evaluate.</param>
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

    /// <summary>
    /// Sets a custom message on the party screen.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void SetMessageText(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    /// <summary>
    /// Clears any messages displayed on individual party member UI elements.
    /// </summary>
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