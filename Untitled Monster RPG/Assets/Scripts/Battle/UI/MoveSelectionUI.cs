using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Utils.GenericSelectionUI;

public class MoveSelectionUI : SelectionUI<TextSlot>
{
    [SerializeField] List<TextSlot> moveTexts;
    [SerializeField] TextMeshProUGUI spText;
    [SerializeField] TextMeshProUGUI typeText;
    List<Move> _moves;

    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 2);
    }

    public void SetMoves(List<Move> moves)
    {
        _moves = moves;
        SetItems(moveTexts.Take(moves.Count).ToList());

        for (int i = 0; i < moveTexts.Count; ++i)
        {
            if (i < moves.Count)
            {
                moveTexts[i].SetText(moves[i].Base.Name);
            }
            else
            {
                moveTexts[i].SetText("-");
            }
        }
    }

    public override void UpdateSelectionInUI()
    {
        base.UpdateSelectionInUI();
        var move = _moves[selectedItem];

        spText.text = $"SP {move.SP}/{move.Base.SP}";
        typeText.text = move.Base.Type.ToString();
        if (move.SP == 0)
        {
            spText.color = GlobalSettings.Instance.EmptyColor;
        }
        else
        {
            spText.color = GlobalSettings.Instance.InactiveColor;
        }
    }
}
