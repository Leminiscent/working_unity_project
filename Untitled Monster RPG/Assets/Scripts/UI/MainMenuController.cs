using System.Linq;
using UnityEngine.SceneManagement;
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
            SceneManager.LoadScene(1);
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
