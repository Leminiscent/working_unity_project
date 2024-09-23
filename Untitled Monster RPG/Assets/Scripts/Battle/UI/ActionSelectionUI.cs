using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class ActionSelectionUI : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 3);
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }
}
