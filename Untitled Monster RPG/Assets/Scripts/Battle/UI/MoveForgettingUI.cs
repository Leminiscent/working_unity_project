using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class MoveForgettingUI : SelectionUI<TextSlot>
{
    [SerializeField] private List<TextMeshProUGUI> _moveTexts;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            _moveTexts[i].text = currentMoves[i].Name;
        }

        _moveTexts[currentMoves.Count].text = newMove.Name;
        SetItems(_moveTexts.Select(m => m.GetComponent<TextSlot>()).ToList());
    }
}
