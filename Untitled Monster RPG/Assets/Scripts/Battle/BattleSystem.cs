using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public enum BattleAction { Fight, Talk, UseItem, SwitchMonster, Run }
public enum BattleTrigger { Desert, DesertOasis, Field, FieldLake, Meadow, Mountain, MountainClouds, Wasteland }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image enemyImage;
    [SerializeField] MoveForgettingUI moveForgettingUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip masterBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite desertBackground;
    [SerializeField] Sprite desertOasisBackground;
    [SerializeField] Sprite fieldBackground;
    [SerializeField] Sprite fieldLakeBackground;
    [SerializeField] Sprite meadowBackground;
    [SerializeField] Sprite mountainBackground;
    [SerializeField] Sprite mountainCloudsBackground;
    [SerializeField] Sprite wastelandBackground;

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
    PlayerController player;
    BattleTrigger battleTrigger;
    Dictionary<BattleTrigger, Sprite> backgroundMapping;

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

        this.PlayerParty = playerParty;
        this.WildMonster = wildMonster;

        battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty, BattleTrigger trigger)
    {
        IsMasterBattle = true;

        this.PlayerParty = playerParty;
        this.EnemyParty = enemyParty;

        player = playerParty.GetComponent<PlayerController>();
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
            playerImage.sprite = player.Sprite;
            enemyImage.sprite = Enemy.Sprite;

            yield return dialogueBox.TypeDialogue(Enemy.Name + " wants to battle!");

            enemyImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyMonster = EnemyParty.GetHealthyMonster();

            enemyUnit.Setup(enemyMonster);
            yield return dialogueBox.TypeDialogue(Enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerMonster = PlayerParty.GetHealthyMonster();

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
        PlayerParty.Monsters.ForEach(m => m.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
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
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMonster);
        dialogueBox.SetMoveNames(newMonster.Moves);
        yield return dialogueBox.TypeDialogue("Go " + newMonster.Base.Name + "!");
    }

    public IEnumerator SendNextMasterMonster()
    {
        var nextMonster = EnemyParty.GetHealthyMonster();

        enemyUnit.Setup(nextMonster);
        yield return dialogueBox.TypeDialogue(Enemy.Name + " sent out " + nextMonster.Base.Name + "!");
    }

    public BattleDialogueBox DialogueBox => dialogueBox;
    public PartyScreen PartyScreen => partyScreen;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}