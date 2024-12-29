using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

public class RecruitmentState : State<BattleSystem>
{
    [SerializeField] private AnswerSelectionUI _selectionUI;

    private BattleSystem _battleSystem;
    private BattleDialogueBox _dialogueBox;
    private List<RecruitmentQuestion> _questions;
    private List<RecruitmentQuestion> _selectedQuestions;
    private int _currentQuestionIndex;
    private bool _yesSelected;
    private float _selectionTimer = 0;

    public static RecruitmentState Instance { get; private set; }
    public BattleUnit TargetUnit { get; set; }

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
        _questions = TargetUnit.Monster.Base.RecruitmentQuestions.OrderBy(static q => Random.value).ToList();
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
        TargetUnit.Monster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return TargetUnit.Hud.SetAffinitySmooth();

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
            yield return AttemptRecruitment();
        }
    }

    private string GenerateReaction(int affinityScore)
    {
        return affinityScore == 2
            ? TargetUnit.Monster.Base.Name + " seems to love your answer!"
            : affinityScore == 1
                ? TargetUnit.Monster.Base.Name + " seems to like your answer."
                : affinityScore == -1
                            ? TargetUnit.Monster.Base.Name + " seems to dislike your answer..."
                            : TargetUnit.Monster.Base.Name + " seems to hate your answer!";
    }

    private IEnumerator AttemptRecruitment()
    {
        // Calculate recruitment chance
        float a = Mathf.Min(Mathf.Max(TargetUnit.Monster.AffinityLevel - 3, 0), 3) * ((3 * TargetUnit.Monster.MaxHp) - (2 * TargetUnit.Monster.Hp)) * TargetUnit.Monster.Base.RecruitRate * ConditionsDB.GetStatusBonus(TargetUnit.Monster.Status) / (3 * TargetUnit.Monster.MaxHp);
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
            yield return _dialogueBox.TypeDialogue(TargetUnit.Monster.Base.Name + " wants to join your party! Will you accept?");

            // Present choice to accept or reject
            _dialogueBox.EnableDialogueText(false);
            _dialogueBox.EnableChoiceBox(true);
            _yesSelected = true;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }
        else
        {
            yield return _dialogueBox.TypeDialogue(TargetUnit.Monster.Base.Name + " refused to join you.");
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    private IEnumerator ProcessAcceptReject(int selection)
    {
        _dialogueBox.EnableDialogueText(true);
        if (selection == 0)
        {
            // Yes
            yield return _dialogueBox.TypeDialogue($"{TargetUnit.Monster.Base.Name} was recruited!");
            _battleSystem.PlayerParty.AddMonster(TargetUnit.Monster);
            _battleSystem.BattleOver(true);
        }
        else
        {
            // No
            yield return _dialogueBox.TypeDialogue($"{TargetUnit.Monster.Base.Name} was rejected.");
            _battleSystem.StateMachine.ChangeState(RunTurnState.Instance);
        }
    }

    private void HandleChoiceBoxInput()
    {
        const float SELECTION_SPEED = 5f;
        float v = Input.GetAxisRaw("Vertical");

        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }

        if (_selectionTimer == 0 && Mathf.Abs(v) > 0.2f)
        {
            _yesSelected = !_yesSelected;
            _selectionTimer = 1 / SELECTION_SPEED;
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
