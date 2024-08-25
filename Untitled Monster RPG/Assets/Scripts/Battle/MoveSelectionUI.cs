using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    int currentSelection = 0;

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ++currentSelection;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            --currentSelection;
        }
        currentSelection = Mathf.Clamp(currentSelection, 0, MonsterBase.MaxMoveCount);
        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int selection)
    {
        for (int i = 0; i < MonsterBase.MaxMoveCount + 1; i++)
        {
            if (i == selection)
            {
                moveTexts[i].color = GlobalSettings.Instance.ActiveColor;
            }
            else
            {
                moveTexts[i].color = GlobalSettings.Instance.InactiveColor;
            }
        }
    }
}
