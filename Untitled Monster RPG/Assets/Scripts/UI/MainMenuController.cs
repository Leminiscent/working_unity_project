using System.Linq;
using Utils.GenericSelectionUI;

public class MainMenuController : SelectionUI<TextSlot>
{
    private void Start()
    {
        SetItems(GetComponentsInChildren<TextSlot>().ToList());

        OnSelected += OnItemSelected;
    }

    private void Update()
    {
        HandleUpdate();
    }

    void OnItemSelected(int selection)
    {
        if (selection == 0)
        {
            // Continue
        }
        else if (selection == 1)
        {
            // New Game
        }
        else if (selection == 2)
        {
            // Quit
        }
    }
}
