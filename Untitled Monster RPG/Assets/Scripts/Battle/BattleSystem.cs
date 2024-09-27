using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public enum BattleAction { Fight, Talk, UseItem, SwitchMonster, Run }
public enum BattleTrigger { Ground, Water }

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
    [SerializeField] Sprite groundBackground;
    [SerializeField] Sprite waterBackground;

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
    public bool IsMasterBattle { get; private set; }
    public int EscapeAttempts { get; set; }
    public MasterController Enemy { get; private set; }

    RecruitmentQuestion currentQuestion;
    int questionIndex;
    int currentAnswer;
    bool currentChoice;
    PlayerController player;
    BattleTrigger battleTrigger;

    public void StartWildBattle(MonsterParty playerParty, Monster wildMonster, BattleTrigger trigger = BattleTrigger.Ground)
    {
        IsMasterBattle = false;

        this.PlayerParty = playerParty;
        this.WildMonster = wildMonster;

        battleTrigger = trigger;
        AudioManager.Instance.PlayMusic(wildBattleMusic);
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty, BattleTrigger trigger = BattleTrigger.Ground)
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

        backgroundImage.sprite = (battleTrigger == BattleTrigger.Ground) ? groundBackground : waterBackground;

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

    IEnumerator AttemptRecruitment(Monster targetMonster)
    {
        float a = Mathf.Min(Mathf.Max(targetMonster.AffinityLevel - 3, 0), 3) * (3 * targetMonster.MaxHp - 2 * targetMonster.HP) * targetMonster.Base.RecruitRate * ConditionsDB.GetStatusBonus(targetMonster.Status) / (3 * targetMonster.MaxHp);
        bool isRecruited;

        if (a >= 255)
        {
            isRecruited = true;
        }
        else
        {
            float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

            isRecruited = UnityEngine.Random.Range(0, 65536) < b;
        }

        if (isRecruited)
        {
            yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " wants to join your party! Will you accept?");
            // ChoiceSelection();

            if (currentChoice)
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was recruited!");
                PlayerParty.AddMonster(enemyUnit.Monster);
                BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was rejected.");
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " refused to join you.");
        }
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    IEnumerator ProcessAnswerSelection()
    {
        var selectedAnswer = currentQuestion.Answers[currentAnswer];

        enemyUnit.Monster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return enemyUnit.Hud.SetAffinitySmooth();

        dialogueBox.EnableDialogueText(true);
        yield return dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        if (questionIndex < 2)
        {
            questionIndex++;
            currentAnswer = 0;
        }
        else
        {
            yield return AttemptRecruitment(enemyUnit.Monster);
        }
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

    string GenerateReaction(int affinityScore)
    {
        if (affinityScore == 2)
        {
            return enemyUnit.Monster.Base.Name + " seems to love your answer!";
        }
        else if (affinityScore == 1)
        {
            return enemyUnit.Monster.Base.Name + " seems to like your answer.";
        }
        else if (affinityScore == -1)
        {
            return enemyUnit.Monster.Base.Name + " seems to dislike your answer...";
        }
        else
        {
            return enemyUnit.Monster.Base.Name + " seems to hate your answer!";
        }
    }

    public BattleDialogueBox DialogueBox => dialogueBox;
    public PartyScreen PartyScreen => partyScreen;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;
    public AudioClip BattleVictoryMusic => battleVictoryMusic;
}