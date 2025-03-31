using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.GenericSelectionUI;

public class MoveForgettingUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;

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