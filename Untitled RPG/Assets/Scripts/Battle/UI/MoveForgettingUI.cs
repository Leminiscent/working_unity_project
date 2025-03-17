using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

/// <summary>
/// MoveForgettingUI is responsible for displaying the list of current moves along with a new move
/// that a battler is trying to learn. It extends the generic SelectionUI for TextSlot components,
/// allowing the player to choose a move to forget.
/// </summary>
public class MoveForgettingUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;

    /// <summary>
    /// Configures the UI with the current moves and the new move.
    /// The new move is displayed in the slot immediately after the current moves.
    /// </summary>
    /// <param name="currentMoves">A list of MoveBase objects representing the battler's current moves.</param>
    /// <param name="newMove">The new MoveBase object the battler is trying to learn.</param>
    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        // Ensure we have enough UI elements for current moves and the new move.
        int requiredSlots = currentMoves.Count + 1;
        if (_moveTexts.Count < requiredSlots)
        {
            Debug.LogError($"Not enough move text slots! Required: {requiredSlots}, Available: {_moveTexts.Count}");
            return;
        }

        // Set text for current moves.
        for (int i = 0; i < currentMoves.Count; i++)
        {
            _moveTexts[i].text = currentMoves[i].Name;
        }

        // Set text for the new move (displayed after current moves).
        _moveTexts[currentMoves.Count].text = newMove.Name;

        // Convert the TextMeshProUGUI components to TextSlot components and update the selection UI.
        List<TextSlot> textSlots = _moveTexts.Select(static m => m.GetComponent<TextSlot>()).ToList();
        SetItems(textSlots);
    }
}
