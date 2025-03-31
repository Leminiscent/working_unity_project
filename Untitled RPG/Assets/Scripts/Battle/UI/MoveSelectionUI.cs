using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Util.GenericSelectionUI;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextSlot> _moveTexts;
    [SerializeField] private TextMeshProUGUI _spText;
    [SerializeField] private TextMeshProUGUI _typeText;

    private List<Move> _moves;

    private void Start()
    {
        // Setup selection UI with a grid layout of 2 columns.
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetMoves(List<Move> moves)
    {
        _moves = moves;
        _selectedItem = 0;

        // Warn if there are more moves than available UI slots.
        if (moves.Count > _moveTexts.Count)
        {
            Debug.LogWarning($"There are {moves.Count} moves but only {_moveTexts.Count} text slots available.");
        }

        // Use only as many text slots as moves available.
        SetItems(_moveTexts.Take(moves.Count).ToList());

        // Update each text slot with the move name, or show "-" if no move is available.
        for (int i = 0; i < _moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                _moveTexts[i].SetText(moves[i].Base.Name);
            }
            else
            {
                _moveTexts[i].SetText("-");
            }
        }
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();

        // Ensure there is a valid move selected.
        if (_moves == null || _moves.Count == 0 || _selectedItem < 0 || _selectedItem >= _moves.Count)
        {
            Debug.LogWarning("UpdateSelectionInUI called with invalid move data.");
            return;
        }

        // Retrieve the currently selected move.
        Move move = _moves[_selectedItem];

        // Update the SP and type text fields.
        _spText.text = $"SP {move.Sp}/{move.Base.SP}";
        _typeText.text = move.Base.Type.ToString();

        // Change SP text color based on availability.
        _spText.color = move.Sp == 0 ? GlobalSettings.Instance.EmptyColor : GlobalSettings.Instance.InactiveColor;
    }
}