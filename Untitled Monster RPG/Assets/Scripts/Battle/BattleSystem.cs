using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RecruitmentSelection, RunningRecruitment, RunningTurn, Busy, PartyScreen, ChoiceSelection, ForgettingMove, BattleOver }
public enum BattleAction { Fight, Talk, UseItem, SwitchMonster, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image enemyImage;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    RecruitmentQuestion currentQuestion;
    int questionIndex;
    int currentAnswer;
    bool currentChoice;
    int escapeAttempts;
    MoveBase moveToLearn;
    MonsterParty playerParty;
    MonsterParty enemyParty;
    Monster wildMonster;
    bool isMasterBattle;
    PlayerController player;
    MasterController enemy;

    public void StartWildBattle(MonsterParty playerParty, Monster wildMonster)
    {
        isMasterBattle = false;
        this.playerParty = playerParty;
        this.wildMonster = wildMonster;
        StartCoroutine(SetupBattle());
    }

    public void StartMasterBattle(MonsterParty playerParty, MonsterParty enemyParty)
    {
        isMasterBattle = true;
        this.playerParty = playerParty;
        this.enemyParty = enemyParty;
        player = playerParty.GetComponent<PlayerController>();
        enemy = enemyParty.GetComponent<MasterController>();
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isMasterBattle)
        {
            playerUnit.Setup(playerParty.GetHealthyMonster());
            enemyUnit.Setup(wildMonster);

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

            var enemyMonster = enemyParty.GetHealthyMonster();

            enemyUnit.Setup(enemyMonster);
            yield return dialogueBox.TypeDialogue(enemy.Name + " sent out " + enemyMonster.Base.Name + "!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerMonster = playerParty.GetHealthyMonster();

            playerUnit.Setup(playerMonster);
            yield return dialogueBox.TypeDialogue("Go " + playerMonster.Base.Name + "!");
            dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogueBox.EnableDialogueText(true);
        dialogueBox.SetDialogue("Choose an action!");
        dialogueBox.EnableActionSelector(true);
    }

    void ChoiceSelection()
    {
        dialogueBox.CalledFrom = state;
        currentChoice = true;
        dialogueBox.EnableChoiceBox(true);
        state = BattleState.ChoiceSelection;
    }

    void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Monsters);
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Fight)
        {
            playerUnit.Monster.CurrentMove = playerUnit.Monster.Moves[currentMove];
            enemyUnit.Monster.CurrentMove = enemyUnit.Monster.GetRandomMove();

            int playerMovePriority = playerUnit.Monster.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Monster.CurrentMove.Base.Priority;
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Monster.Speed >= enemyUnit.Monster.Speed;
            }

            var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
            var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;
            var secondMonster = secondUnit.Monster;

            yield return RunMove(firstUnit, secondUnit, firstUnit.Monster.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if (secondMonster.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Monster.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchMonster)
            {
                var selectedMonster = partyScreen.SelectedMember;

                dialogueBox.EnableActionSelector(false);
                yield return SwitchMonster(selectedMonster);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                dialogueBox.EnableActionSelector(false);
                yield return dialogueBox.TypeDialogue("No items yet!");
            }
            else if (playerAction == BattleAction.Talk)
            {
                dialogueBox.EnableActionSelector(false);
                yield return RunRecruitment();
            }
            else if (playerAction == BattleAction.Run)
            {
                dialogueBox.EnableActionSelector(false);
                yield return AttemptEscape();
            }

            var enemyMove = enemyUnit.Monster.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Monsters.ForEach(m => m.OnBattleOver());
        OnBattleOver(won);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Monster.OnStartOfTurn();

        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Monster);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }

        yield return ShowStatusChanges(sourceUnit.Monster);
        move.AP--;
        yield return dialogueBox.TypeDialogue(sourceUnit.Monster.Base.Name + " used " + move.Base.Name + "!");

        if (CheckIfMoveHits(move, sourceUnit.Monster, targetUnit.Monster))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Monster, targetUnit.Monster, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Monster.TakeDamage(move, sourceUnit.Monster);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Monster.HP > 0)
            {
                foreach (var effect in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= effect.Chance)
                    {
                        yield return RunMoveEffects(effect, sourceUnit.Monster, targetUnit.Monster, effect.Target);
                    }
                }
            }

            if (targetUnit.Monster.HP <= 0)
            {
                yield return HandleMonsterDefeat(targetUnit);
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue(sourceUnit.Monster.Base.Name + "'s attack missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Monster source, Monster target, MoveTarget moveTarget)
    {
        // Stat Boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunRecruitment()
    {
        state = BattleState.RunningRecruitment;

        if (isMasterBattle)
        {
            yield return dialogueBox.TypeDialogue("You can't recruit another Master's monster!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogueBox.TypeDialogue("You want to talk?");
        yield return dialogueBox.TypeDialogue("Alright, let's talk!");

        List<RecruitmentQuestion> questions = enemyUnit.Monster.Base.RecruitmentQuestions;
        List<RecruitmentQuestion> selectedQuestions = new List<RecruitmentQuestion>();

        while (selectedQuestions.Count < 3)
        {
            var question = questions[UnityEngine.Random.Range(0, questions.Count)];

            if (!selectedQuestions.Contains(question))
            {
                selectedQuestions.Add(question);
            }
        }

        questionIndex = 0;
        foreach (var question in selectedQuestions)
        {
            currentQuestion = question;
            yield return dialogueBox.TypeDialogue(question.Question);
            dialogueBox.EnableDialogueText(false);
            dialogueBox.SetAnswers(question.Answers);
            currentAnswer = 0;
            dialogueBox.EnableAnswerSelector(true);
            state = BattleState.RecruitmentSelection;
            yield return new WaitUntil(() => state == BattleState.RunningRecruitment);
        }

        state = BattleState.RunningTurn;
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
            yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " wants to join your party. Will you accept?");
            ChoiceSelection();
            yield return new WaitUntil(() => state == BattleState.Busy);

            if (currentChoice)
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was recruited!");
                playerParty.AddMonster(enemyUnit.Monster);
                BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " was rejected.");
                state = BattleState.RunningRecruitment;
            }
        }
        else
        {
            yield return dialogueBox.TypeDialogue(enemyUnit.Monster.Base.Name + " refused to join you.");
            state = BattleState.RunningRecruitment;
        }
    }

    IEnumerator AttemptEscape()
    {
        state = BattleState.Busy;

        if (isMasterBattle)
        {
            yield return dialogueBox.TypeDialogue("You can't run from a Master battle!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Monster.Speed;
        int enemySpeed = enemyUnit.Monster.Speed;

        if (playerSpeed >= enemySpeed)
        {
            yield return dialogueBox.TypeDialogue("You got away safely!");
            BattleOver(true);
        }
        else
        {
            float f = ((playerSpeed * 128) / enemySpeed + 30 * escapeAttempts) % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogueBox.TypeDialogue("You got away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogueBox.TypeDialogue("You couldn't get away!");
                state = BattleState.RunningTurn;
            }
        }
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        sourceUnit.Monster.OnEndOfTurn();
        yield return ShowStatusChanges(sourceUnit.Monster);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Monster.HP <= 0)
        {
            yield return HandleMonsterDefeat(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Monster source, Monster target)
    {
        if (move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Monster monster)
    {
        while (monster.StatusChanges.Count > 0)
        {
            var message = monster.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
    }

    IEnumerator HandleMonsterDefeat(BattleUnit defeatedUnit)
    {
        yield return dialogueBox.TypeDialogue(defeatedUnit.Monster.Base.Name + " has been defeated!");
        defeatedUnit.PlayDefeatAnimation();
        yield return new WaitForSeconds(2f);

        if (!defeatedUnit.IsPlayerUnit)
        {
            int expYield = defeatedUnit.Monster.Base.ExpYield;
            int enemyLevel = defeatedUnit.Monster.Level;
            float masterBonus = isMasterBattle ? 1.5f : 1f;
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * masterBonus / 7);

            playerUnit.Monster.Exp += expGain;
            yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " gained " + expGain + " experience!");
            yield return playerUnit.Hud.SetExpSmooth();
            while (playerUnit.Monster.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " grew to level " + playerUnit.Monster.Level + "!");

                var newMove = playerUnit.Monster.GetLearnableMoveAtCurrentLevel();

                if (newMove != null)
                {
                    if (playerUnit.Monster.Moves.Count < MonsterBase.MaxMoveCount)
                    {
                        playerUnit.Monster.LearnMove(newMove);
                        yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " learned " + newMove.Base.Name + "!");
                        dialogueBox.SetMoveNames(playerUnit.Monster.Moves);
                    }
                    else
                    {
                        yield return dialogueBox.TypeDialogue(playerUnit.Monster.Base.Name + " is trying to learn " + newMove.Base.Name + "!");
                        yield return dialogueBox.TypeDialogue("But it already knows four moves!");
                        yield return ChooseMoveToForget(playerUnit.Monster, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.ForgettingMove);
                        yield return new WaitForSeconds(2f);
                    }
                }
                yield return playerUnit.Hud.SetExpSmooth(true);
            }
            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(defeatedUnit);
    }

    void CheckForBattleOver(BattleUnit defeatedUnit)
    {
        if (defeatedUnit.IsPlayerUnit)
        {
            var nextMonster = playerParty.GetHealthyMonster();
            if (nextMonster != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            if (!isMasterBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextMonster = enemyParty.GetHealthyMonster();
                if (nextMonster != null)
                {
                    StartCoroutine(SendNextMasterMonster());
                }
                else
                {
                    BattleOver(true);
                }
            }
        }
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);
        dialogueBox.EnableMoveSelector(true);
    }

    IEnumerator ChooseMoveToForget(Monster monster, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogueBox.TypeDialogue($"Choose a move for {monster.Base.Name} to forget.");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(monster.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.ForgettingMove;
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
        {
            yield return dialogueBox.TypeDialogue("A critical hit!");
        }

        if (damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogueBox.TypeDialogue("It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogueBox.TypeDialogue("It's not very effective!");
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if (state == BattleState.RecruitmentSelection)
        {
            HandleRecruitmentSelection();
        }
        else if (state == BattleState.ChoiceSelection)
        {
            HandleChoiceSelection();
        }
        else if (state == BattleState.ForgettingMove)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
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
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
                StartCoroutine(RunTurns(BattleAction.Talk));
            }
            else if (currentAction == 2)
            {
                // Item
                StartCoroutine(RunTurns(BattleAction.UseItem));
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
                StartCoroutine(RunTurns(BattleAction.Run));
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

            if (move.AP == 0) return;
            dialogueBox.EnableMoveSelector(false);
            dialogueBox.EnableDialogueText(true);
            StartCoroutine(RunTurns(BattleAction.Fight));
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
        state = BattleState.Busy;

        var selectedAnswer = currentQuestion.Answers[currentAnswer];

        enemyUnit.Monster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return enemyUnit.Hud.SetAffinitySmooth();

        dialogueBox.EnableDialogueText(true);
        yield return dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        if (questionIndex < 2)
        {
            questionIndex++;
            currentAnswer = 0;
            state = BattleState.RunningRecruitment;
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
            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchMonster));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchMonster(selectedMember));
            }
            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Monster.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose a monster!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
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

    IEnumerator SwitchMonster(Monster newMonster)
    {
        state = BattleState.Busy;
        if (playerUnit.Monster.HP > 0)
        {
            yield return dialogueBox.TypeDialogue("Come back " + playerUnit.Monster.Base.Name + "!");
            playerUnit.PlayExitAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMonster);
        dialogueBox.SetMoveNames(newMonster.Moves);
        yield return dialogueBox.TypeDialogue("Go " + newMonster.Base.Name + "!");

        state = BattleState.RunningTurn;
    }

    IEnumerator SendNextMasterMonster()
    {
        state = BattleState.Busy;

        var nextMonster = enemyParty.GetHealthyMonster();

        enemyUnit.Setup(nextMonster);
        yield return dialogueBox.TypeDialogue(enemy.Name + " sent out " + nextMonster.Base.Name + "!");
        state = BattleState.RunningTurn;
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
}