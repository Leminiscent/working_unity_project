using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.StateMachine;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    private DeputyController deputyController;
    MasterController master;

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
        deputyController = FindObjectOfType<DeputyController>().GetComponent<DeputyController>();
    }

    private void Start()
    {
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(FreeRoamState.Instance);
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();
        DialogueManager.Instance.OnShowDialogue += () => { StateMachine.Push(DialogueState.Instance); };
        DialogueManager.Instance.OnDialogueFinished += () => { StateMachine.Pop(); };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            StateMachine.Push(PauseState.Instance);
        }
        else
        {
            StateMachine.Pop();
        }
    }

    public void StartWildBattle()
    {
        BattleState.Instance.Trigger = CurrentScene.GetComponent<MapArea>().Terrain;
        StateMachine.Push(BattleState.Instance);
    }

    public void StartMasterBattle(MasterController master)
    {
        BattleState.Instance.Trigger = CurrentScene.GetComponent<MapArea>().Terrain;
        BattleState.Instance.Master = master;
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
    }

    public void SetCurrentScene(SceneDetails scene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = scene;
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
        // GUILayout.Label("STATE STACK", style);
        // foreach (var state in StateMachine.StateStack)
        // {
        //     GUILayout.Label(state.GetType().ToString(), style);
        // }
    }

    public PlayerController PlayerController { get => playerController; set => playerController = value; }
    public Camera WorldCamera => worldCamera;
    public DeputyController DeputyController { get => deputyController; set => deputyController = value; }
    public PartyScreen PartyScreen => partyScreen;
}
