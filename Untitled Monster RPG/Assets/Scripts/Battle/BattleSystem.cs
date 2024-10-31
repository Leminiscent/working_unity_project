using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleDialogueBox dialogueBox;
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private Image playerImage;
    [SerializeField] private Image enemyImage;
    [SerializeField] private MoveForgettingUI moveForgettingUI;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] private AudioClip wildBattleMusic;
    [SerializeField] private AudioClip masterBattleMusic;
    [SerializeField] private AudioClip battleVictoryMusic;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite desertBackground;
    [SerializeField] private Sprite desertOasisBackground;
    [SerializeField] private Sprite fieldBackground;
    [SerializeField] private Sprite fieldLakeBackground;
    [SerializeField] private Sprite meadowBackground;
    [SerializeField] private Sprite mountainBackground;
    [SerializeField] private Sprite mountainCloudsBackground;
    [SerializeField] private Sprite wastelandBackground;
    private BattleTrigger battleTrigger;
    private Dictionary<BattleTrigger, Sprite> backgroundMapping;

    public StateMachine<BattleSystem> StateMachine { get; private set; }
    public event Action<bool> OnBattleOver;
    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Monster SelectedMonster { get; set; }
    public ItemBase SelectedItem { get; set; }
    public bool BattleIsOver { get; private set; }
    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty EnemyParty { get; private set; }
    public Monster WildMonster { get; private set; }
    public Field Field { get; private set; }
    public bool IsMasterBattle { get; private set; }
    public int EscapeAttempts { get; set; }
    public MasterController Enemy { get; private set; }
    public PlayerController Player { get; private set; }
    public BattleDialogueBox DialogueBox => dialogueBox;
    public PartyScreen PartyScreen => partyScreen;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;

    private void Awake()
    {
        backgroundMapping = new Dictionary<BattleTrigger, Sprite>
        {
            { BattleTrigger.Desert, desertBackground },
            { BattleTrigger.DesertOasis, desertOasisBackground },
            { BattleTrigger.Field, fieldBackground },
            { BattleTrigger.FieldLake, fieldLakeBackground },
            { BattleTrigger.Meadow, meadowBackground },
            { BattleTrigger.Mountain, mountainBackground },
            { BattleTrigger.MountainClouds, mountainCloudsBackground },
            { BattleTrigger.Wasteland, wastelandBackground }
        };
    }

    public void StartWildBattle(MonsterParty playerParty, Monster wildMonster, BattleTrigger trigger)
    {
        IsMasterBattle = false;

        PlayerParty = playerParty;
        WildMonster = wildMonster;

        battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty, BattleTrigger trigger)
    {
        IsMasterBattle = true;

        PlayerParty = playerParty;
        EnemyParty = enemyParty;

        Player = playerParty.GetComponent<PlayerController>();
        Enemy = enemyParty.GetComponent<MasterController>();
        battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(masterBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        playerUnit.Clear();
        enemyUnit.Clear();

        if (backgroundMapping.ContainsKey(battleTrigger))
        {
            backgroundImage.sprite = backgroundMapping[battleTrigger];
        }
        else
        {
            backgroundImage.sprite = fieldBackground; // Fallback option
        }

        if (!IsMasterBattle)
        {
            playerUnit.Setup(PlayerParty.GetHealthyMonster());
            enemyUnit.Setup(WildMonster);

            dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
            yield return dialogueBox.TypeDialogue("A wild " + enemyUnit.Monster.Base.Name + " appeared!");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            enemyImage.gameObject.SetActive(true);
            playerImage.sprite = Player.Sprite;
            enemyImage.sprite = Enemy.Sprite;

            yield return dialogueBox.TypeDialogue(Enemy.Name + " wants to battle!");

            enemyImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            Monster enemyMonster = EnemyParty.GetHealthyMonster();

            enemyUnit.Setup(enemyMonster);
            yield return dialogueBox.TypeDialogue(Enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            Monster playerMonster = PlayerParty.GetHealthyMonster();

            playerUnit.Setup(playerMonster);
            yield return dialogueBox.TypeDialogue("Go " + playerMonster.Base.Name + "!");
            dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
        }

        Field = new Field();
        BattleIsOver = false;
        EscapeAttempts = 0;
        partyScreen.Init();
        StateMachine.ChangeState(ActionSelectionState.Instance);
    }

    public void BattleOver(bool won)
    {
        BattleIsOver = true;
        PlayerParty.Monsters.ForEach(static m => m.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        ActionSelectionState.Instance.SelectionUI.ResetSelection();
        OnBattleOver(won);
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchMonster(Monster newMonster)
    {
        if (playerUnit.Monster.HP > 0)
        {
            yield return dialogueBox.TypeDialogue("Come back " + playerUnit.Monster.Base.Name + "!");
            playerUnit.PlayExitAnimation();
            yield return new WaitForSeconds(0.75f);
        }

        playerUnit.Setup(newMonster);
        dialogueBox.SetMoveNames(newMonster.Moves);
        yield return dialogueBox.TypeDialogue("Go " + newMonster.Base.Name + "!");
    }

    public IEnumerator SendNextMasterMonster()
    {
        Monster nextMonster = EnemyParty.GetHealthyMonster();

        enemyUnit.Setup(nextMonster);
        yield return dialogueBox.TypeDialogue(Enemy.Name + " sent out " + nextMonster.Base.Name + "!");
    }
}

public enum BattleAction
{
    Fight,
    Talk,
    UseItem,
    SwitchMonster,
    Run
}

public enum BattleTrigger
{
    Desert,
    DesertOasis,
    Field,
    FieldLake,
    Meadow,
    Mountain,
    MountainClouds,
    Wasteland
}