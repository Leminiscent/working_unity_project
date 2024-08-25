using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialogue, Cutscene, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    GameState state;
    GameState prevState;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        MonsterDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;
        DialogueManager.Instance.OnShowDialogue += () => state = GameState.Dialogue;
        DialogueManager.Instance.OnCloseDialogue += () =>
        {
            if (state == GameState.Dialogue)
            {
                state = GameState.FreeRoam;
            }
        };
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

    public void StartWildBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<MonsterParty>();
        var wildMonster = CurrentScene.GetComponent<MapArea>().GetRandomWildMonster();
        var wildMonsterCopy = new Monster(wildMonster.Base, wildMonster.Level);

        battleSystem.StartWildBattle(playerParty, wildMonsterCopy);
    }

    MasterController master;

    public void StartMasterBattle(MasterController master)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        this.master = master;

        var playerParty = playerController.GetComponent<MonsterParty>();
        var enemyParty = master.GetComponent<MonsterParty>();

        battleSystem.StartMasterBattle(playerParty, enemyParty);
    }

    public void OnEnterMasterView(MasterController master)
    {
        state = GameState.Cutscene;
        StartCoroutine(master.TriggerBattle(playerController));
    }

    void EndBattle(bool won)
    {
        if (master != null && won == true)
        {
            master.BattleLost();
            master = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.S))
            {
                SavingSystem.i.Save("saveSlot1");
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                SavingSystem.i.Load("saveSlot1");
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialogue)
        {
            DialogueManager.Instance.HandleUpdate();
        }
    }

    public void SetCurrentScene(SceneDetails scene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = scene;
    }
}
