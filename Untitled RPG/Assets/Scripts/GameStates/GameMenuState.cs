using System;
using System.Collections;
using UnityEngine;
using Util.StateMachine;

public class GameMenuState : State<GameController>
{
    [SerializeField] private MenuController _menuController;

    private GameController _gameController;

    public static GameMenuState Instance { get; private set; }

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
    }

    public override void Enter(GameController owner)
    {
        _gameController = owner;
        if (_menuController != null)
        {
            _ = StartCoroutine(ObjectUtil.ScaleIn(_menuController.gameObject));
            _menuController.OnSelected += OnMenuItemSelected;
            _menuController.OnBack += OnBack;
        }
        else
        {
            Debug.LogError("MenuController reference is missing in GameMenuState.");
        }
    }

    public override void Execute()
    {
        if (_menuController != null)
        {
            _menuController.HandleUpdate();
        }
    }

    public override void Exit()
    {
        if (_menuController != null)
        {
            _ = StartCoroutine(ObjectUtil.ScaleOut(_menuController.gameObject));
            _menuController.OnSelected -= OnMenuItemSelected;
            _menuController.OnBack -= OnBack;
        }
    }

    private void OnMenuItemSelected(int selection)
    {
        switch (selection)
        {
            case 0: // Party
                StartCoroutine(ProcessSelection(() => _gameController.StateMachine.Push(PartyState.Instance)));
                break;
            case 1: // Inventory
                StartCoroutine(ProcessSelection(() => _gameController.StateMachine.Push(InventoryState.Instance)));
                break;
            case 2: // Storage
                StartCoroutine(ProcessSelection(() => _gameController.StateMachine.Push(StorageState.Instance)));
                break;
            case 3: // Save
                StartCoroutine(ProcessSelection(static () => SavingSystem.Instance.Save("saveSlot1")));
                break;
            case 4: // Load
                StartCoroutine(ProcessSelection(static () => SavingSystem.Instance.Load("saveSlot1")));
                break;
            case 5: // Quit
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
            default:
                Debug.LogWarning($"Invalid menu selection: {selection}");
                break;
        }
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    private IEnumerator ProcessSelection(Action action)
    {
        _menuController.EnableInput(false);
        yield return Fader.Instance.FadeIn(0.5f);

        action?.Invoke();

        _gameController.StateMachine.Push(CutsceneState.Instance);
        yield return Fader.Instance.FadeOut(0.5f);
        
        _gameController.StateMachine.Pop();
        _menuController.EnableInput(true);
    }

    private void OnBack()
    {
        if (_menuController != null)
        {
            _menuController.ResetSelection();
        }
        AudioManager.Instance.PlaySFX(AudioID.UIReturn);
        _gameController.StateMachine.Pop();
    }
}