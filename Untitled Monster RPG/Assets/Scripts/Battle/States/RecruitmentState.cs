using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RecruitmentState : State<BattleSystem>
{
    [SerializeField] AnswerSelectionUI selectionUI;
    BattleSystem battleSystem;
    Monster enemyMonster;
    BattleDialogueBox dialogueBox;

    List<RecruitmentQuestion> questions;
    List<RecruitmentQuestion> selectedQuestions;
    int currentQuestionIndex;
    bool yesSelected;
    float selectionTimer = 0;

    public static RecruitmentState Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Enter(BattleSystem owner)
    {
        battleSystem = owner;
        enemyMonster = battleSystem.EnemyUnit.Monster;
        dialogueBox = battleSystem.DialogueBox;
        StartCoroutine(StartRecruitment());
    }

    public override void Execute()
    {
        if (selectionUI.gameObject.activeInHierarchy)
        {
            selectionUI.HandleUpdate();
        }
        else if (dialogueBox.IsChoiceBoxEnabled)
        {
            HandleChoiceBoxInput();
        }
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnAnswerSelected;
    }

    IEnumerator StartRecruitment()
    {
        if (battleSystem.IsMasterBattle)
        {
            yield return dialogueBox.TypeDialogue("You can't recruit another Master's monster!");
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
            yield break;
        }

        yield return dialogueBox.TypeDialogue("You want to talk?");
        yield return dialogueBox.TypeDialogue("Alright, let's talk!");

        // Select 3 random questions
        questions = enemyMonster.Base.RecruitmentQuestions.OrderBy(q => Random.value).ToList();
        selectedQuestions = questions.Take(3).ToList();
        currentQuestionIndex = 0;
        yield return PresentQuestion();
    }

    IEnumerator PresentQuestion()
    {
        var currentQuestion = selectedQuestions[currentQuestionIndex];

        yield return dialogueBox.TypeDialogue(currentQuestion.QuestionText);

        // Set up the answer selection UI
        dialogueBox.EnableDialogueText(false);
        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnAnswerSelected;
        selectionUI.SetAnswers(currentQuestion.Answers);
        selectionUI.UpdateSelectionInUI();
    }

    void OnAnswerSelected(int selection)
    {
        StartCoroutine(ProcessAnswer(selection));
    }

    IEnumerator ProcessAnswer(int selectedAnswerIndex)
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnAnswerSelected;

        var currentQuestion = selectedQuestions[currentQuestionIndex];
        var selectedAnswer = currentQuestion.Answers[selectedAnswerIndex];

        // Update affinity level
        enemyMonster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return battleSystem.EnemyUnit.Hud.SetAffinitySmooth();

        // Show reaction
        dialogueBox.EnableDialogueText(true);
        yield return dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        // Proceed to next question or attempt recruitment
        if (currentQuestionIndex < selectedQuestions.Count - 1)
        {
            currentQuestionIndex++;
            yield return PresentQuestion();
        }
        else
        {
            // Attempt recruitment
            yield return AttemptRecruitment(enemyMonster);
        }
    }

    string GenerateReaction(int affinityScore)
    {
        if (affinityScore == 2)
        {
            return enemyMonster.Base.Name + " seems to love your answer!";
        }
        else if (affinityScore == 1)
        {
            return enemyMonster.Base.Name + " seems to like your answer.";
        }
        else if (affinityScore == -1)
        {
            return enemyMonster.Base.Name + " seems to dislike your answer...";
        }
        else
        {
            return enemyMonster.Base.Name + " seems to hate your answer!";
        }
    }

    IEnumerator AttemptRecruitment(Monster targetMonster)
    {
        // Calculate recruitment chance
        float a = Mathf.Min(Mathf.Max(targetMonster.AffinityLevel - 3, 0), 3) * (3 * targetMonster.MaxHp - 2 * targetMonster.HP) * targetMonster.Base.RecruitRate * ConditionsDB.GetStatusBonus(targetMonster.Status) / (3 * targetMonster.MaxHp);
        bool canRecruit;

        if (a >= 255)
        {
            canRecruit = true;
        }
        else
        {
            float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

            canRecruit = Random.Range(0, 65536) < b;
        }

        if (canRecruit)
        {
            yield return dialogueBox.TypeDialogue(enemyMonster.Base.Name + " wants to join your party! Will you accept?");

            // Present choice to accept or reject
            dialogueBox.EnableDialogueText(false);
            dialogueBox.EnableChoiceBox(true);
            yesSelected = true;
            dialogueBox.UpdateChoiceBox(yesSelected);
        }
        else
        {
            yield return dialogueBox.TypeDialogue(enemyMonster.Base.Name + " refused to join you.");
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    IEnumerator ProcessAcceptReject(int selection)
    {
        dialogueBox.EnableDialogueText(true);
        if (selection == 0)
        {
            // Yes
            yield return dialogueBox.TypeDialogue($"{enemyMonster.Base.Name} was recruited!");
            battleSystem.PlayerParty.AddMonster(enemyMonster);
            battleSystem.BattleOver(true);
        }
        else
        {
            // No
            yield return dialogueBox.TypeDialogue($"{enemyMonster.Base.Name} was rejected.");
            battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    void HandleChoiceBoxInput()
    {
        const float selectionSpeed = 5f;
        float v = Input.GetAxis("Vertical");

        if (selectionTimer > 0)
        {
            selectionTimer = Mathf.Clamp(selectionTimer - Time.deltaTime, 0, selectionTimer);
        }

        if (selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
        {
            yesSelected = !yesSelected;
            selectionTimer = 1 / selectionSpeed;
            dialogueBox.UpdateChoiceBox(yesSelected);
        }

        if (Input.GetButtonDown("Action"))
        {
            dialogueBox.EnableChoiceBox(false);

            int selection = yesSelected ? 0 : 1;

            StartCoroutine(ProcessAcceptReject(selection));
        }

        if (Input.GetButtonDown("Back"))
        {
            dialogueBox.EnableChoiceBox(false);
            StartCoroutine(ProcessAcceptReject(1));
        }
    }
}
