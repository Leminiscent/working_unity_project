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
    public BattleUnit RecruitTarget { get; set; }

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
            _battleSystem.StateMachine.Pop();
            yield break;
        }

        yield return _dialogueBox.TypeDialogue("You want to talk?");
        yield return _dialogueBox.TypeDialogue("Alright, let's talk!");

        // Select 3 random questions
        _questions = RecruitTarget.Monster.Base.RecruitmentQuestions.OrderBy(static q => Random.value).ToList();
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
        RecruitTarget.Monster.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        yield return RecruitTarget.Hud.SetAffinitySmooth();

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
            ? RecruitTarget.Monster.Base.Name + " seems to love your answer!"
            : affinityScore == 1
                ? RecruitTarget.Monster.Base.Name + " seems to like your answer."
                : affinityScore == -1
                            ? RecruitTarget.Monster.Base.Name + " seems to dislike your answer..."
                            : RecruitTarget.Monster.Base.Name + " seems to hate your answer!";
    }

    private IEnumerator AttemptRecruitment()
    {
        // Calculate recruitment chance
        float a = Mathf.Min(Mathf.Max(RecruitTarget.Monster.AffinityLevel - 3, 0), 3) * ((3 * RecruitTarget.Monster.MaxHp) - (2 * RecruitTarget.Monster.Hp)) * RecruitTarget.Monster.Base.RecruitRate * ConditionsDB.GetStatusBonus(RecruitTarget.Monster.Status) / (3 * RecruitTarget.Monster.MaxHp);
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
            yield return _dialogueBox.TypeDialogue(RecruitTarget.Monster.Base.Name + " wants to join your party! Will you accept?");

            // Present choice to accept or reject
            _dialogueBox.EnableDialogueText(false);
            _dialogueBox.EnableChoiceBox(true);
            _yesSelected = true;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }
        else
        {
            yield return _dialogueBox.TypeDialogue(RecruitTarget.Monster.Base.Name + " refused to join you.");
            _battleSystem.StateMachine.Pop();
        }
    }

    private IEnumerator ProcessAcceptReject(int selection)
    {
        _dialogueBox.EnableDialogueText(true);

        if (selection == 0)
        {
            // Yes
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Monster.Base.Name} was recruited!");

            RecruitTarget.PlayExitAnimation();
            RecruitTarget.Hud.gameObject.SetActive(false);
            _battleSystem.EnemyUnits.Remove(RecruitTarget);
            _battleSystem.PlayerParty.AddMonster(RecruitTarget.Monster);
            yield return new WaitForSeconds(0.75f);

            if (_battleSystem.EnemyUnits.Count == 0)
            {
                _battleSystem.BattleOver(true);
            }
            else
            {
                BattleAction defeatedUnitAction = RunTurnState.Instance.BattleActions.FirstOrDefault(a => a.SourceUnit == RecruitTarget);
                if (defeatedUnitAction != null)
                {
                    defeatedUnitAction.IsValid = false;
                }

                List<BattleAction> actionsToAdjust = RunTurnState.Instance.BattleActions.Where(a => a.TargetUnits != null && a.TargetUnits.Contains(RecruitTarget)).ToList();
                foreach (BattleAction a in actionsToAdjust)
                {
                    a.TargetUnits.Remove(RecruitTarget);
                    if (a.TargetUnits.Count == 0)
                    {
                        a.TargetUnits.Add(_battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]);
                    }
                }
            }
        }
        else
        {
            // No
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Monster.Base.Name} was rejected.");
        }

        _battleSystem.StateMachine.Pop();
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
