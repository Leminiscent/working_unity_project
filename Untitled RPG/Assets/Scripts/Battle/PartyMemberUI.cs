using TMPro;
using UnityEngine;

/// <summary>
/// Manages the UI display for an individual party member, showing their name, level, HP bar, and a message.
/// </summary>
public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private HPBar _hpBar;

    private Battler _battler;

    /// <summary>
    /// Initializes the UI with the given battler data.
    /// </summary>
    /// <param name="battler">The battler associated with this party member.</param>
    public void Init(Battler battler)
    {
        _battler = battler;
        UpdateData();
        SetMessage("");
        if (_battler != null)
        {
            _battler.OnHPChanged += UpdateData;
        }
    }

    /// <summary>
    /// Updates the UI elements based on the battler's current data.
    /// </summary>
    private void UpdateData()
    {
        if (_battler == null)
        {
            Debug.LogWarning("Battler data is missing in PartyMemberUI.UpdateData.");
            return;
        }

        if (_nameText != null)
        {
            _nameText.text = _battler.Base.Name;
        }

        if (_levelText != null)
        {
            _levelText.text = $"Lvl {_battler.Level}";
        }

        if (_hpBar != null)
        {
            _hpBar.SetHP((float)_battler.Hp / _battler.MaxHp);
        }
    }

    /// <summary>
    /// Sets the color of the party member's name to indicate selection.
    /// </summary>
    /// <param name="selected">True to set as selected, false to set as unselected.</param>
    public void SetSelected(bool selected)
    {
        if (_nameText != null && GlobalSettings.Instance != null)
        {
            _nameText.color = selected ? GlobalSettings.Instance.ActiveColor : GlobalSettings.Instance.InactiveColor;
        }
    }

    /// <summary>
    /// Displays the specified message in the UI.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void SetMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }
}
