using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public enum GameState { FreeRoam, Battle, Dialogue, Menu, PartyScreen, Inventory, Cutscene, Paused, Transformation, Shop }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    GameState state;
    GameState prevState;
    GameState stateBeforeTransformation;

    public StateMachine<GameController> StateMachine { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MonsterDB.Init();
        MoveDB.Init();
        ItemDB.Init();
        QuestDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(FreeRoamState.Instance);
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();
        DialogueManager.Instance.OnShowDialogue += () => { StateMachine.Push(DialogueState.Instance); };
        DialogueManager.Instance.OnDialogueFinished += () => { StateMachine.Pop(); };
        ShopController.Instance.OnStart += () => state = GameState.Shop;
        ShopController.Instance.OnFinish += () => state = GameState.FreeRoam;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartWildBattle(BattleTrigger trigger)
    {
        BattleState.Instance.trigger = trigger;
        StateMachine.Push(BattleState.Instance);
    }

    MasterController master;

    public void StartMasterBattle(MasterController master)
    {
        BattleState.Instance.master = master;
        StateMachine.Push(BattleState.Instance);
    }

    public void OnEnterMasterView(MasterController master)
    {
        StartCoroutine(master.TriggerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (master != null && won == true)
        {
            master.BattleLost();
            master = null;
        }
        partyScreen.SetPartyData();
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<MonsterParty>();
        bool hasTransformations = playerParty.CheckForTransformations();

        if (hasTransformations)
        {
            StartCoroutine(playerParty.RunTransformations());
        }
        else
        {
            AudioManager.Instance.PlayMusic(CurrentScene.SceneMusic, fade: true);
        }
    }

    private void Update()
    {
        StateMachine.Execute();

        if (state == GameState.Cutscene)
        {
            playerController.Character.HandleUpdate();
        }
        else if (state == GameState.Dialogue)
        {
            DialogueManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Shop)
        {
            ShopController.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails scene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = scene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Inventory;
        }
        else if (selectedItem == 2)
        {
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.Instance.FadeIn(0.5f);
        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);
        if (waitForFadeOut)
        {
            yield return Fader.Instance.FadeOut(0.5f);
        }
        else
        {
            StartCoroutine(Fader.Instance.FadeOut(0.5f));
        }
    }

    private void OnGUI()
    {
        var style = new GUIStyle();

        style.fontSize = 40;
        GUILayout.Label("STATE STACK", style);
        foreach (var state in StateMachine.StateStack)
        {
            GUILayout.Label(state.GetType().ToString(), style);
        }
    }

    public GameState State => state;
    public PlayerController PlayerController => playerController;
    public Camera WorldCamera => worldCamera;
    public PartyScreen PartyScreen => partyScreen;
}
