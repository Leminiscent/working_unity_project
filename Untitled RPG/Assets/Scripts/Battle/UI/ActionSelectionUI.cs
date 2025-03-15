using System.Linq;
using Utils.GenericSelectionUI;

public class ActionSelectionUI : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetSelectionSettings(SelectionType.Grid, 3);
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }

    protected override void PlayShiftAudio(int previousSelection, int newSelection)
    {
        if (previousSelection == 4 && newSelection == 1)
        {
            return;
        }
        base.PlayShiftAudio(previousSelection, newSelection);
    }
}