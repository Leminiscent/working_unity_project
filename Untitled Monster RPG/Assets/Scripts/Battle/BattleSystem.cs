using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.StateMachine;

public enum BattleStates { Start, ActionSelection, MoveSelection, RecruitmentSelection, RunningRecruitment, RunningTurn, Busy, Inventory, PartyScreen, ChoiceSelection, ForgettingMove, BattleOver }
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
    public bool BattleIsOver { get; private set; }
    public MonsterParty PlayerParty { get; private set; }
    public MonsterParty EnemyParty { get; private set; }
    public Monster WildMonster { get; private set; }
    public bool IsMasterBattle { get; private set; }
    public int EscapeAttempts { get; set; }

    BattleStates state;
    int currentAction;
    int currentMove;
    RecruitmentQuestion currentQuestion;
    int questionIndex;
    int currentAnswer;
    bool currentChoice;
    MoveBase moveToLearn;
    PlayerController player;
    MasterController enemy;
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
        enemy = enemyParty.GetComponent<MasterController>();
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
            enemyImage.sprite = enemy.Sprite;

            yield return dialogueBox.TypeDialogue(enemy.Name + " wants to battle!");

            enemyImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyMonster = EnemyParty.GetHealthyMonster();

            enemyUnit.Setup(enemyMonster);
            yield return dialogueBox.TypeDialogue(enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

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

    void ActionSelection()
    {
        state = BattleStates.ActionSelection;
        dialogueBox.EnableDialogueText(true);
        dialogueBox.SetDialogue("Choose an action!");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenInventory()
    {
        state = BattleStates.Inventory;
        inventoryUI.gameObject.SetActive(true);
    }

    void ChoiceSelection()
    {
        dialogueBox.CalledFrom = state;
        currentChoice = true;
        dialogueBox.EnableChoiceBox(true);
        state = BattleStates.ChoiceSelection;
    }

    void OpenPartyScreen()
    {
        // partyScreen.CalledFrom = state;
        state = BattleStates.PartyScreen;
        partyScreen.gameObject.SetActive(true);
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
            ChoiceSelection();
            yield return new WaitUntil(() => state == BattleStates.Busy);

            if (currentChoice)
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was recruited!");
                PlayerParty.AddMonster(enemyUnit.Monster);
                BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was rejected.");
                state = BattleStates.RunningRecruitment;
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " refused to join you.");
            state = BattleStates.RunningRecruitment;
        }
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleStates.Busy;
        inventoryUI.gameObject.SetActive(false);

        yield break; // yield return StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    void MoveSelection()
    {
        state = BattleStates.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove)
    {
        state = BattleStates.Busy;
        yield return dialogueBox.TypeDialogue($"Choose a move for {monster.Base.Name} to forget.");
        moveForgettingUI.gameObject.SetActive(true);
        moveForgettingUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleStates.ForgettingMove;
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();


        if (state == BattleStates.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleStates.RecruitmentSelection)
        {
            HandleRecruitmentSelection();
        }
        else if (state == BattleStates.Inventory)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleStates.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            // inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleStates.ChoiceSelection)
        {
            HandleChoiceSelection();
        }
        else if (state == BattleStates.ForgettingMove)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveForgettingUI.gameObject.SetActive(false);
                if (moveIndex == MonsterBase.MaxMoveCount)
                {
                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} did not learn {moveToLearn.Name}!"));
                }
                else
                {
                    var selectedMove = playerUnit.Monster.Moves[moveIndex].Base;

                    StartCoroutine(dialogueBox.TypeDialogue($"{playerUnit.Monster.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));
                    playerUnit.Monster.Moves[moveIndex] = new Move(moveToLearn);
                    dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
                }

                moveToLearn = null;
                state = BattleStates.RunningTurn;
            };

            // moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentAction += 3;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentAction -= 3;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 5);

        dialogueBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                // Talk
                // StartCoroutine(RunTurns(BattleAction.Talk));
            }
            else if (currentAction == 2)
            {
                // Item
                OpenInventory();
            }
            else if (currentAction == 3)
            {
                // Guard
            }
            else if (currentAction == 4)
            {
                // Switch
                OpenPartyScreen();
            }
            else if (currentAction == 5)
            {
                // Run
                // StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentMove < playerUnit.Monster.Moves.Count - 2)
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && currentMove > 1)
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Monster.Moves.Count - 1);
        dialogueBox.UpdateMoveSelction(currentMove, playerUnit.Monster.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Monster.Moves[currentMove];

            if (move.SP == 0) return;
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            // StartCoroutine(RunTurns(BattleAction.Fight));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableActionSelector(true);
            ActionSelection();
        }
    }

    void HandleRecruitmentSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ++currentAnswer;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            --currentAnswer;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && currentAnswer < 2)
        {
            currentAnswer += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && currentAnswer > 1)
        {
            currentAnswer -= 2;
        }
        currentAnswer = Mathf.Clamp(currentAnswer, 0, 3);
        dialogueBox.UpdateAnswerSelection(currentAnswer);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnableAnswerSelector(false);
            StartCoroutine(ProcessAnswerSelection());
        }
    }

    IEnumerator ProcessAnswerSelection()
    {
        state = BattleStates.Busy;

        var selectedAnswer = currentQuestion.Answers[currentAnswer];

        enemyUnit.Monster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return enemyUnit.Hud.SetAffinitySmooth();

        dialogueBox.EnableDialogueText(true);
        yield return dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        if (questionIndex < 2)
        {
            questionIndex++;
            currentAnswer = 0;
            state = BattleStates.RunningRecruitment;
        }
        else
        {
            yield return AttemptRecruitment(enemyUnit.Monster);
        }
    }

    void HandlePartySelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;

            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText(selectedMember.Base.Name + " is unable to fight!");
                return;
            }
            if (selectedMember == playerUnit.Monster)
            {
                partyScreen.SetMessageText(selectedMember.Base.Name + " is already in battle!");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            // if (partyScreen.CalledFrom == BattleStates.ActionSelection)
            // {
            //     StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            // }
            // else
            // {
            //     state = BattleStates.Busy;
            //     StartCoroutine(SwitchMonster(selectedMember));
            // }
            // partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            // ActionSelection();
            // partyScreen.CalledFrom = null;
        };

        // partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleChoiceSelection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentChoice = !currentChoice;
        }

        dialogueBox.UpdateChoiceBox(currentChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogueBox.EnableChoiceBox(false);
            state = dialogueBox.CalledFrom.Value;
            dialogueBox.CalledFrom = null;
        }
    }

    public IEnumerator SwitchMonster(Monster newMonster)
    {
        state = BattleStates.Busy;
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

    IEnumerator SendNextMasterMonster()
    {
        state = BattleStates.Busy;

        var nextMonster = EnemyParty.GetHealthyMonster();

        enemyUnit.Setup(nextMonster);
        yield return dialogueBox.TypeDialogue(enemy.Name + " sent out " + nextMonster.Base.Name + "!");
        state = BattleStates.RunningTurn;
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