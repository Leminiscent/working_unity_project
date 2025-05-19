using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Util.StateMachine;

public class GameController : MonoBehaviour
{
    [field: SerializeField, FormerlySerializedAs("_playerController")] public PlayerController PlayerController { get; private set; }
    [SerializeField] private BattleSystem _battleSystem;
    [field: SerializeField, FormerlySerializedAs("_worldCamera")] public Camera WorldCamera { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_partyScreen")] public PartyScreen PartyScreen { get; private set; }
    [SerializeField] private InventoryUI _inventoryUI;

    public StateMachine<GameController> StateMachine { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    public static GameController Instance { get; private set; }

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

        // Lock and hide cursor.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize databases.
        BattlerDB.Init();
        MoveDB.Init();
        ItemDB.Init();
        QuestDB.Init();
        StatusConditionDB.Init();
        WeatherConditionDB.Init();
    }

    private void Start()
    {
        // Initialize state machine.
        StateMachine = new StateMachine<GameController>(this);
        StateMachine.ChangeState(PauseState.Instance);

        // Subscribe to battle system event.
        if (_battleSystem != null)
        {
            _battleSystem.OnBattleOver += EndBattle;
        }
        else
        {
            Debug.LogError("BattleSystem reference is missing in GameController.");
        }

        // Initialize party screen.
        if (PartyScreen != null)
        {
            PartyScreen.Init();
        }
        else
        {
            Debug.LogError("PartyScreen reference is missing in GameController.");
        }

        // Subscribe to dialogue events.
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnShowDialogue += OnShowDialogue;
            DialogueManager.Instance.OnDialogueFinished += OnDialogueFinished;
        }
        else
        {
            Debug.LogWarning("DialogueManager instance not found.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to avoid memory leaks.
        if (_battleSystem != null)
        {
            _battleSystem.OnBattleOver -= EndBattle;
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnShowDialogue -= OnShowDialogue;
            DialogueManager.Instance.OnDialogueFinished -= OnDialogueFinished;
        }
    }

    private void Update()
    {
        // Update state machine.
        StateMachine?.Execute();
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

    private void OnShowDialogue()
    {
        StateMachine.Push(DialogueState.Instance);
    }

    private void OnDialogueFinished()
    {
        StateMachine.Pop();
    }

    public void StartRogueBattle()
    {
        if (CurrentScene != null)
        {
            BattleState.Instance.Trigger = CurrentScene.GetComponent<MapArea>().Terrain;
            StateMachine.Push(BattleState.Instance);
        }
        else
        {
            Debug.LogWarning("CurrentScene is not set. Cannot start battle.");
        }
    }

    public void StartCommanderBattle(CommanderController commander)
    {
        if (CurrentScene != null)
        {
            BattleState.Instance.Trigger = CurrentScene.GetComponent<MapArea>().Terrain;
            BattleState.Instance.Commander = commander;
            StateMachine.Push(BattleState.Instance);
        }
        else
        {
            Debug.LogWarning("CurrentScene is not set. Cannot start battle.");
        }
    }

    public void OnEnterCommanderView(CommanderController commander)
    {
        _ = StartCoroutine(commander.TriggerBattle(PlayerController));
    }

    private void EndBattle(bool won)
    {
        if (won)
        {
            if (PartyScreen != null)
            {
                PartyScreen.SetPartyData();
            }

            BattleParty playerParty = PlayerController.GetComponent<BattleParty>();
            bool hasTransformations = playerParty.CheckForTransformations();

            if (hasTransformations)
            {
                _ = StartCoroutine(playerParty.RunTransformations());
            }
            else
            {
                AudioManager.Instance.PlayMusic(CurrentScene.SceneMusic, fade: true);
            }

            // Reset battler leveling flags.
            foreach (Battler battler in playerParty.Battlers)
            {
                battler.HasJustLeveledUp = false;
            }
        }
        else
        {
            _ = StartCoroutine(PerformGameOver());
        }
    }

    public void SetCurrentScene(SceneDetails scene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = scene;
    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool waitForFadeOut = false)
    {
        yield return Fader.Instance.FadeIn(0.5f);
        WorldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);
        if (waitForFadeOut)
        {
            yield return Fader.Instance.FadeOut(0.5f);
        }
        else
        {
            _ = StartCoroutine(Fader.Instance.FadeOut(0.5f));
        }
    }

    private IEnumerator PerformGameOver()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded && scene.name != "Main Menu")
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        EssentialObjects essentialObjects = FindObjectOfType<EssentialObjects>();
        if (essentialObjects != null)
        {
            Destroy(essentialObjects.gameObject);
        }

        SceneManager.LoadScene("Main Menu");
    }

    // private void OnGUI()
    // {
    //     GUIStyle style = new() { fontSize = 40 };
    //     GUILayout.Label("STATE STACK", style);
    //     foreach (State<GameController> state in StateMachine.StateStack)
    //     {
    //         GUILayout.Label(state.GetType().ToString(), style);
    //     }
    // }
}