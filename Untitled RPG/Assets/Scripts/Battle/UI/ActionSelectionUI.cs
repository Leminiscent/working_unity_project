using System.Linq;
using Utils.GenericSelectionUI;

/// <summary>
/// ActionSelectionUI is responsible for displaying action selection options in a grid layout and
/// handling user interactions with the selection UI. It inherits from the generic SelectionUI class,
/// specialized for TextSlot components.
/// </summary>
public class ActionSelectionUI : SelectionUI<TextSlot>
{
    /// <summary>
    /// Initializes the ActionSelectionUI by setting up selection settings and retrieving TextSlot items.
    /// </summary>
    private void Start()
    {
        // Configure the selection UI to use a grid layout with 3 columns.
        SetSelectionSettings(SelectionType.Grid, 3);

        // Retrieve all TextSlot components that are children of this GameObject and set them as selectable items.
        SetItems(GetComponentsInChildren<TextSlot>().ToList());
    }

    /// <summary>
    /// Plays shift audio when the selection changes.
    /// Skips playing audio when transitioning from "Switch" to "Talk" if the unit is not the commander.
    /// </summary>
    /// <param name="previousSelection">The previously selected index.</param>
    /// <param name="newSelection">The new selected index.</param>
    protected override void PlayShiftAudio(int previousSelection, int newSelection)
    {
        if (!BattleState.Instance.BattleSystem.SelectingUnit.Battler.IsCommander && previousSelection == 4 && newSelection == 1)
        {
            return;
        }
        base.PlayShiftAudio(previousSelection, newSelection);
    }
}
