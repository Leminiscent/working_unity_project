using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.GenericSelectionUI;

public class MainMenuController : SelectionUI<TextSlot>
{
    [SerializeField] private GameObject _selections;

    private bool _hasSave;

    private void Start()
    {
        _hasSave = SavingSystem.Instance.CheckForExistingSave("saveSlot1");
        InitializeMenuItems();
        OnSelected += OnItemSelected;
    }

    private void OnDestroy()
    {
        OnSelected -= OnItemSelected;
    }

    private void InitializeMenuItems()
    {
        List<TextSlot> textSlots = _selections.GetComponentsInChildren<TextSlot>().ToList();

        if (_hasSave)
        {
            SetItems(textSlots);
        }
        else
        {
            SetItems(textSlots.TakeLast(2).ToList());
            // Disable the "Continue" option by setting its color to the empty color.
            textSlots.First().GetComponent<TextMeshProUGUI>().color = GlobalSettings.Instance.EmptyColor;
        }
    }

    private void Update()
    {
        HandleUpdate();
    }

    private void OnItemSelected(int selection)
    {
        // Adjust selection index if no save exists.
        if (!_hasSave)
        {
            selection++;
        }

        switch (selection)
        {
            case 0:
                _ = StartCoroutine(ContinueSelected());
                break;
            case 1:
                _ = StartCoroutine(NewGameSelected());
                break;
            case 2:
                QuitGame();
                break;
            default:
                break;
        }
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator PrepareSceneChange()
    {
        EnableInput(false);
        yield return Fader.Instance.FadeIn(0.5f);
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator ContinueSelected()
    {
        yield return PrepareSceneChange();

        GameController.Instance.StateMachine.ChangeState(CutsceneState.Instance);
        SceneManager.LoadScene(1);
        SavingSystem.Instance.Load("saveSlot1");

        yield return new WaitForSeconds(0.25f);
        yield return Fader.Instance.FadeOut(0.5f);
        GameController.Instance.StateMachine.ChangeState(FreeRoamState.Instance);

        Destroy(gameObject);
    }

    private IEnumerator NewGameSelected()
    {
        yield return PrepareSceneChange();

        GameController.Instance.StateMachine.ChangeState(CharacterSelectState.Instance);
        Destroy(gameObject);
    }
}