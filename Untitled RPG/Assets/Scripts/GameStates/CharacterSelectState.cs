using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.StateMachine;

public class CharacterSelectState : State<GameController>
{
    [SerializeField] private CharacterSelectScreen _characterSelectScreen;
    [SerializeField] private List<Battler> _availableBattlers;

    private GameController _gameController;

    public static CharacterSelectState Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Initialize all available battlers.
        if (_availableBattlers != null)
        {
            foreach (Battler battler in _availableBattlers)
            {
                battler.InitBattler();
            }
        }
        else
        {
            Debug.LogWarning("Available battlers list is null.");
        }
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _ = StartCoroutine(EnterFromMainMenu());
    }

    public override void Execute()
    {
        if (_characterSelectScreen != null)
        {
            _characterSelectScreen.HandleUpdate();
        }
    }

    public override void Exit()
    {
        if (_characterSelectScreen != null)
        {
            _characterSelectScreen.OnSelected -= OnCharacterSelected;
            _characterSelectScreen.gameObject.SetActive(false);
        }
    }

    private IEnumerator EnterFromMainMenu()
    {
        if (_characterSelectScreen != null)
        {
            _characterSelectScreen.EnableInput(false);
            _characterSelectScreen.gameObject.SetActive(true);
            
            _characterSelectScreen.SetAvailableBattlers(_availableBattlers);
            _characterSelectScreen.OnSelected += OnCharacterSelected;

            yield return Fader.Instance.FadeOut(0.5f);
            _characterSelectScreen.EnableInput(true);
        }
        else
        {
            Debug.LogError("CharacterSelectScreen reference is missing.");
        }
    }

    private void OnCharacterSelected(int selectionIndex)
    {
        if (selectionIndex < 0 || selectionIndex >= _availableBattlers.Count)
        {
            Debug.LogError("Invalid character selection index.");
            return;
        }

        Battler selectedBattler = _availableBattlers[selectionIndex];
        PlayerController.Instance.SetPlayerBattler(selectedBattler);
        AudioManager.Instance.PlaySFX(AudioID.UISelect);

        // Start the transition to the next game state.
        _ = StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        _characterSelectScreen.EnableInput(false);
        yield return Fader.Instance.FadeIn(0.5f);

        _gameController.StateMachine.ChangeState(CutsceneState.Instance);
        SavingSystem.Instance.Delete("saveSlot1");
        SceneManager.LoadScene(1);

        yield return new WaitForSeconds(0.25f);
        yield return Fader.Instance.FadeOut(0.5f);

        _gameController.StateMachine.ChangeState(FreeRoamState.Instance);
    }
}