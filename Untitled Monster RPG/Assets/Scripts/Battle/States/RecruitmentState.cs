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
    bool isAcceptRejectPhase = false;

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

        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnAnswerSelected;

        // Start the recruitment process
        battleSystem.StartCoroutine(StartRecruitment());
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
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
            yield return new WaitForSeconds(1f);
            battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
            yield break;
        }

        yield return dialogueBox.TypeDialogue("You want to talk?");
        yield return dialogueBox.TypeDialogue("Alright, let's talk!");

        // Select 3 random questions
        questions = enemyMonster.Base.RecruitmentQuestions;
        selectedQuestions = new List<RecruitmentQuestion>();

        while (selectedQuestions.Count < 3)
        {
            var question = questions[Random.Range(0, questions.Count)];

            if (!selectedQuestions.Contains(question))
            {
                selectedQuestions.Add(question);
            }
        }

        currentQuestionIndex = 0;
        yield return PresentQuestion();
    }

    IEnumerator PresentQuestion()
    {
        var currentQuestion = selectedQuestions[currentQuestionIndex];

        yield return dialogueBox.TypeDialogue(currentQuestion.QuestionText);
        // Set up the answer selection UI
        dialogueBox.EnableDialogueText(false);
        selectionUI.SetAnswers(currentQuestion.Answers);
        selectionUI.UpdateSelectionInUI();
    }

    void OnAnswerSelected(int selection)
    {
        if (isAcceptRejectPhase)
        {
            battleSystem.StartCoroutine(ProcessAcceptReject(selection));
        }
        else
        {
            battleSystem.StartCoroutine(ProcessAnswer(selection));
        }
    }


    IEnumerator ProcessAnswer(int selectedAnswerIndex)
    {
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
        bool isRecruited;

        if (a >= 255)
        {
            isRecruited = true;
        }
        else
        {
            float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

            isRecruited = Random.Range(0, 65536) < b;
        }

        if (isRecruited)
        {
            yield return dialogueBox.TypeDialogue(enemyMonster.Base.Name + " wants to join your party! Will you accept?");
            // Present choice to accept or reject
            isAcceptRejectPhase = true;
            dialogueBox.EnableDialogueText(false);
            selectionUI.SetAcceptRejectOptions();
            selectionUI.UpdateSelectionInUI();
        }
        else
        {
            yield return dialogueBox.TypeDialogue(enemyMonster.Base.Name + " refused to join you.");
            battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }

    IEnumerator ProcessAcceptReject(int selection)
    {
        dialogueBox.EnableDialogueText(true);
        if (selection == 0) // Yes
        {
            yield return dialogueBox.TypeDialogue($"{enemyMonster.Base.Name} was recruited!");
            battleSystem.PlayerParty.AddMonster(enemyMonster);
            battleSystem.BattleOver(true);
        }
        else // No
        {
            yield return dialogueBox.TypeDialogue($"{enemyMonster.Base.Name} was rejected.");
            battleSystem.StateMachine.ChangeState(ActionSelectionState.Instance);
        }
    }
}
