using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextSlot> _moveTexts;
    [SerializeField] private TextMeshProUGUI _spText;
    [SerializeField] private TextMeshProUGUI _typeText;

    private List<Move> _moves;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetMoves(List<Move> moves)
    {
        _moves = moves;
        selectedItem = 0;
        SetItems(_moveTexts.Take(moves.Count).ToList());

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
        Move move = _moves[selectedItem];

        _spText.text = $"SP {move.Sp}/{move.Base.SP}";
        _typeText.text = move.Base.Type.ToString();
        _spText.color = move.Sp == 0 ? GlobalSettings.Instance.EmptyColor : GlobalSettings.Instance.InactiveColor;
    }
}
