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
        // Prevents the audio from playing when a unit is not the commander and the selection is moved from Switch to Talk
        if (!BattleState.Instance.BattleSystem.SelectingUnit.Battler.IsCommander && previousSelection == 4 && newSelection == 1)
        {
            return;
        }
        base.PlayShiftAudio(previousSelection, newSelection);
    }
}
