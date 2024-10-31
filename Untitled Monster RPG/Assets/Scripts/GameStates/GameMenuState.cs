using System.Collections;
using UnityEngine;
using Utils.StateMachine;

public class GameMenuState : State<GameController>
{
    [SerializeField] MenuController menuController;
    GameController gameController;

    public static GameMenuState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(GameController owner)
    {
        gameController = owner;
        menuController.gameObject.SetActive(true);
        menuController.OnSelected += OnMenuItemSelected;
        menuController.OnBack += OnBack;
    }

    public override void Execute()
    {
        menuController.HandleUpdate();
    }

    public override void Exit()
    {
        menuController.gameObject.SetActive(false);
        menuController.OnSelected -= OnMenuItemSelected;
        menuController.OnBack -= OnBack;
    }

    void OnMenuItemSelected(int selection)
    {
        if (selection == 0)
        {
            gameController.StateMachine.Push(PartyState.Instance);
        }
        else if (selection == 1)
        {
            gameController.StateMachine.Push(InventoryState.Instance);
        }
        else if (selection == 2)
        {
            gameController.StateMachine.Push(StorageState.Instance);
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

    IEnumerator SaveSelected()
    {
        yield return Fader.Instance.FadeIn(0.5f);
        SavingSystem.i.Save("saveSlot1");
        yield return Fader.Instance.FadeOut(0.5f);
    }

    IEnumerator LoadSelected()
    {
        yield return Fader.Instance.FadeIn(0.5f);
        SavingSystem.i.Load("saveSlot1");
        yield return Fader.Instance.FadeOut(0.5f);
    }

    void OnBack()
    {
        gameController.StateMachine.Pop();
    }
}
