using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.GenericSelectionUI;

public class MainMenuController : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetItems(GetComponentInChildren<TextSlot>().ToList());
    }
}
