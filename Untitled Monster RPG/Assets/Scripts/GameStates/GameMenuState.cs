using System.Collections;
using UnityEngine;
using Utils.StateMachine;

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
        _menuController.gameObject.SetActive(true);
        _menuController.OnSelected += OnMenuItemSelected;
        _menuController.OnBack += OnBack;
    }

    public override void Execute()
    {
        _menuController.HandleUpdate();
    }

    public override void Exit()
    {
        _menuController.gameObject.SetActive(false);
        _menuController.OnSelected -= OnMenuItemSelected;
        _menuController.OnBack -= OnBack;
    }

    private void OnMenuItemSelected(int selection)
    {
        if (selection == 0)
        {
            _gameController.StateMachine.Push(PartyState.Instance);
        }
        else if (selection == 1)
        {
            _gameController.StateMachine.Push(InventoryState.Instance);
        }
        else if (selection == 2)
        {
            _gameController.StateMachine.Push(StorageState.Instance);
        }
        else if (selection == 3)
        {
            StartCoroutine(SaveSelected());
        }
        else if (selection == 4)
        {
            StartCoroutine(LoadSelected());
        }
    }

    private IEnumerator SaveSelected()
    {
        yield return Fader.Instance.FadeIn(0.5f);
        SavingSystem.i.Save("saveSlot1");
        yield return Fader.Instance.FadeOut(0.5f);
    }

    private IEnumerator LoadSelected()
    {
        yield return Fader.Instance.FadeIn(0.5f);
        SavingSystem.i.Load("saveSlot1");
        yield return Fader.Instance.FadeOut(0.5f);
    }

    private void OnBack()
    {
        _gameController.StateMachine.Pop();
    }
}
