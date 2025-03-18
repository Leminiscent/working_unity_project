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

        foreach (Battler battler in _availableBattlers)
        {
            battler.InitNewBattler();
        }
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        _characterSelectScreen.gameObject.SetActive(true);
        _characterSelectScreen.SetAvailableBattlers(_availableBattlers);
        _characterSelectScreen.OnSelected += OnCharacterSelected;
    }

    public override void Execute()
    {
        _characterSelectScreen.HandleUpdate();
    }

    public override void Exit()
    {
        _characterSelectScreen.OnSelected -= OnCharacterSelected;
        _characterSelectScreen.gameObject.SetActive(false);
    }

    private void OnCharacterSelected(int selectionIndex)
    {
        Battler selectedBattler = _availableBattlers[selectionIndex];
        PlayerController.Instance.SetPlayerBattler(selectedBattler);

        StartCoroutine(StartGame());
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private IEnumerator StartGame()
    {
        yield return Fader.Instance.FadeIn(0.1f);

        _gameController.StateMachine.ChangeState(FreeRoamState.Instance);
        SavingSystem.Instance.Delete("saveSlot1");
        SceneManager.LoadScene(1);

        yield return Fader.Instance.FadeOut(0.75f);
    }
}