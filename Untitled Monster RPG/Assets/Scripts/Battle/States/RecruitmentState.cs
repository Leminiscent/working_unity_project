using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RecruitmentState : State<BattleSystem>
{
    [SerializeField] private AnswerSelectionUI _selectionUI;

    private BattleSystem _battleSystem;
    private Monster _enemyMonster;
    private BattleDialogueBox _dialogueBox;
    private List<RecruitmentQuestion> _questions;
    private List<RecruitmentQuestion> _selectedQuestions;
    private int _currentQuestionIndex;
    private bool _yesSelected;
    private float _selectionTimer = 0;

    public static RecruitmentState Instance { get; private set; }

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
    }

    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _enemyMonster = _battleSystem.EnemyUnit.Monster;
        _dialogueBox = _battleSystem.DialogueBox;
        StartCoroutine(StartRecruitment());
    }

    public override void Execute()
    {
        if (_selectionUI.gameObject.activeInHierarchy)
        {
            _selectionUI.HandleUpdate();
        }
        else if (_dialogueBox.IsChoiceBoxEnabled)
        {
            HandleChoiceBoxInput();
        }
    }

    public override void Exit()
    {
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnAnswerSelected;
    }

    private IEnumerator StartRecruitment()
    {
        if (_battleSystem.IsMasterBattle)
        {
            yield return _dialogueBox.TypeDialogue("You can't recruit another Master's monster!");
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
            yield break;
        }

        yield return _dialogueBox.TypeDialogue("You want to talk?");
        yield return _dialogueBox.TypeDialogue("Alright, let's talk!");

        // Select 3 random questions
        _questions = _enemyMonster.Base.RecruitmentQuestions.OrderBy(static q => Random.value).ToList();
        _selectedQuestions = _questions.Take(3).ToList();
        _currentQuestionIndex = 0;
        yield return PresentQuestion();
    }

    private IEnumerator PresentQuestion()
    {
        RecruitmentQuestion currentQuestion = _selectedQuestions[_currentQuestionIndex];

        yield return _dialogueBox.TypeDialogue(currentQuestion.QuestionText);

        // Set up the answer selection UI
        _dialogueBox.EnableDialogueText(false);
        _selectionUI.gameObject.SetActive(true);
        _selectionUI.OnSelected += OnAnswerSelected;
        _selectionUI.SetAnswers(currentQuestion.Answers);
        _selectionUI.UpdateSelectionInUI();
    }

    private void OnAnswerSelected(int selection)
    {
        StartCoroutine(ProcessAnswer(selection));
    }

    private IEnumerator ProcessAnswer(int selectedAnswerIndex)
    {
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnAnswerSelected;

        RecruitmentQuestion currentQuestion = _selectedQuestions[_currentQuestionIndex];
        RecruitmentAnswer selectedAnswer = currentQuestion.Answers[selectedAnswerIndex];

        // Update affinity level
        _enemyMonster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return _battleSystem.EnemyUnit.Hud.SetAffinitySmooth();

        // Show reaction
        _dialogueBox.EnableDialogueText(true);
        yield return _dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        // Proceed to next question or attempt recruitment
        if (_currentQuestionIndex < _selectedQuestions.Count - 1)
        {
            _currentQuestionIndex++;
            yield return PresentQuestion();
        }
        else
        {
            // Attempt recruitment
            yield return AttemptRecruitment(_enemyMonster);
        }
    }

    private string GenerateReaction(int affinityScore)
    {
        if (affinityScore == 2)
        {
            return _enemyMonster.Base.Name + " seems to love your answer!";
        }
        else if (affinityScore == 1)
        {
            return _enemyMonster.Base.Name + " seems to like your answer.";
        }
        else
        {
            return affinityScore == -1
                ? _enemyMonster.Base.Name + " seems to dislike your answer..."
                : _enemyMonster.Base.Name + " seems to hate your answer!";
        }
    }

    private IEnumerator AttemptRecruitment(Monster targetMonster)
    {
        // Calculate recruitment chance
        float a = Mathf.Min(Mathf.Max(targetMonster.AffinityLevel - 3, 0), 3) * ((3 * targetMonster.MaxHP) - (2 * targetMonster.HP)) * targetMonster.Base.RecruitRate * ConditionsDB.GetStatusBonus(targetMonster.Status) / (3 * targetMonster.MaxHP);
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
            yield return _dialogueBox.TypeDialogue(_enemyMonster.Base.Name + " wants to join your party! Will you accept?");

            // Present choice to accept or reject
            _dialogueBox.EnableDialogueText(false);
            _dialogueBox.EnableChoiceBox(true);
            _yesSelected = true;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }
        else
        {
            yield return _dialogueBox.TypeDialogue(_enemyMonster.Base.Name + " refused to join you.");
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    private IEnumerator ProcessAcceptReject(int selection)
    {
        _dialogueBox.EnableDialogueText(true);
        if (selection == 0)
        {
            // Yes
            yield return _dialogueBox.TypeDialogue($"{_enemyMonster.Base.Name} was recruited!");
            _battleSystem.PlayerParty.AddMonster(_enemyMonster);
            _battleSystem.BattleOver(true);
        }
        else
        {
            // No
            yield return _dialogueBox.TypeDialogue($"{_enemyMonster.Base.Name} was rejected.");
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    private void HandleChoiceBoxInput()
    {
        const float selectionSpeed = 5f;
        float v = Input.GetAxisRaw("Vertical");

        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }

        if (_selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
        {
            _yesSelected = !_yesSelected;
            _selectionTimer = 1 / selectionSpeed;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }

        if (Input.GetButtonDown("Action"))
        {
            _dialogueBox.EnableChoiceBox(false);

            int selection = _yesSelected ? 0 : 1;

            StartCoroutine(ProcessAcceptReject(selection));
        }

        if (Input.GetButtonDown("Back"))
        {
            _dialogueBox.EnableChoiceBox(false);
            StartCoroutine(ProcessAcceptReject(1));
        }
    }
}
