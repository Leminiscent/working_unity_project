using System.Collections;
using UnityEngine;
using Utils.StateMachine;

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private InventoryUI inventoryUI;
    private MasterController master;

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

    private void EndBattle(bool won)
    {
        if (master != null && won == true)
        {
            master.BattleLost();
            master = null;
        }
        partyScreen.SetPartyData();
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        MonsterParty playerParty = playerController.GetComponent<MonsterParty>();
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
        GUIStyle style = new GUIStyle();

        style.fontSize = 40;
        // GUILayout.Label("STATE STACK", style);
        // foreach (var state in StateMachine.StateStack)
        // {
        //     GUILayout.Label(state.GetType().ToString(), style);
        // }
    }

    public PlayerController PlayerController => playerController;
    public Camera WorldCamera => worldCamera;
    public PartyScreen PartyScreen => partyScreen;
}
