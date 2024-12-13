using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.GenericSelectionUI;

public class MainMenuController : SelectionUI<TextSlot>
{
    private void Start()
    {
        var textSlots = GetComponentsInChildren<TextSlot>().ToList();

        if (SavingSystem.Instance.CheckForExistingSave("saveSlot1"))
        {
            SetItems(textSlots);
        }
        else
        {
            SetItems(textSlots.TakeLast(2).ToList());
            textSlots.First().GetComponent<TextMeshProUGUI>().color = GlobalSettings.Instance.EmptyColor;
        }

        OnSelected += OnItemSelected;
    }

    private void Update()
    {
        HandleUpdate();
    }

    void OnItemSelected(int selection)
    {
        if (!SavingSystem.Instance.CheckForExistingSave("saveSlot1"))
        {
            selection++;
        }

        if (selection == 0)
        {
            // Continue
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            GameController.Instance.StateMachine.ChangeState(FreeRoamState.Instance);
            SceneManager.LoadScene(1);
            SavingSystem.Instance.Load("saveSlot1");

            Destroy(gameObject);
        }
        else if (selection == 1)
        {
            // New Game
            GameController.Instance.StateMachine.ChangeState(FreeRoamState.Instance);
            SavingSystem.Instance.Delete("saveSlot1");
            SceneManager.LoadScene(1);
        }
        else if (selection == 2)
        {
            // Quit
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
