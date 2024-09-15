using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

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

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    MenuController menuController;
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        menuController = GetComponent<MenuController>();
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
        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();
        DialogueManager.Instance.OnShowDialogue += () => { prevState = state; state = GameState.Dialogue; };
        DialogueManager.Instance.OnDialogueFinished += () =>
        {
            if (state == GameState.Dialogue)
            {
                state = prevState;
            }
        };

        menuController.OnBack += () =>
        {
            menuController.CloseMenu();
            state = GameState.FreeRoam;
        };
        menuController.OnMenuSelected += OnMenuSelected;

        TransformationManager.Instance.OnStartTransformation += () =>
        {
            stateBeforeTransformation = state;
            state = GameState.Transformation;
        };
        TransformationManager.Instance.OnEndTransformation += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeTransformation;
            AudioManager.Instance.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };

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
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<MonsterParty>();
        var wildMonster = CurrentScene.GetComponent<MapArea>().GetRandomWildMonster(trigger);
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
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
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
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                // TODO
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Inventory)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
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

    public GameState State => state;
}
