using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.StateMachine;

/// <summary>
/// Represents the recruitment state in battle, handling dialogue, question selection, and the recruitment process.
/// </summary>
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

    /// <summary>
    /// Enters the recruitment state, initializing dialogue and starting the recruitment sequence.
    /// </summary>
    /// <param name="owner">The BattleSystem owning this state.</param>
    public override void Enter(BattleSystem owner)
    {
        _battleSystem = owner;
        _dialogueBox = _battleSystem.DialogueBox;

        // Validate references
        if (_selectionUI == null)
        {
            Debug.LogError("AnswerSelectionUI is not assigned.");
            return;
        }
        if (RecruitTarget == null)
        {
            Debug.LogError("RecruitTarget is not assigned.");
            _battleSystem.StateMachine.Pop();
            return;
        }

        StartCoroutine(StartRecruitment());
    }

    /// <summary>
    /// Updates the recruitment state by handling UI input.
    /// </summary>
    public override void Execute()
    {
        if (_selectionUI != null && _selectionUI.gameObject.activeInHierarchy)
        {
            _selectionUI.HandleUpdate();
        }
        else if (_dialogueBox != null && _dialogueBox.IsChoiceBoxEnabled)
        {
            HandleChoiceBoxInput();
        }
    }

    /// <summary>
    /// Exits the recruitment state, cleaning up UI and event subscriptions.
    /// </summary>
    public override void Exit()
    {
        if (_selectionUI != null)
        {
            _selectionUI.gameObject.SetActive(false);
            _selectionUI.OnSelected -= OnAnswerSelected;
        }
    }

    /// <summary>
    /// Begins the recruitment sequence by presenting initial dialogue and selecting recruitment questions.
    /// </summary>
    private IEnumerator StartRecruitment()
    {
        // Prevent recruitment during a commander battle.
        if (_battleSystem.IsCommanderBattle)
        {
            yield return !RecruitTarget.Battler.IsCommander
                ? _dialogueBox.TypeDialogue("You can't recruit another Commander's battler!")
                : _dialogueBox.TypeDialogue("You can't recruit another Commander!");
            _battleSystem.StateMachine.Pop();
            yield break;
        }

        yield return _dialogueBox.TypeDialogue("You want to talk?");
        yield return _dialogueBox.TypeDialogue("Alright, let's talk!");

        // Randomly select 3 recruitment questions.
        _questions = RecruitTarget.Battler.Base.RecruitmentQuestions.OrderBy(static q => Random.value).ToList();
        _selectedQuestions = _questions.Take(3).ToList();
        _currentQuestionIndex = 0;

        RecruitTarget.Hud.ToggleAffinityBar(true);
        yield return PresentQuestion();
    }

    /// <summary>
    /// Presents the current recruitment question and sets up the answer selection UI.
    /// </summary>
    private IEnumerator PresentQuestion()
    {
        RecruitmentQuestion currentQuestion = _selectedQuestions[_currentQuestionIndex];

        yield return _dialogueBox.TypeDialogue(currentQuestion.QuestionText);

        // Set up answer selection UI.
        _dialogueBox.EnableDialogueText(false);
        _selectionUI.gameObject.SetActive(true);
        _selectionUI.OnSelected += OnAnswerSelected;
        _selectionUI.SetAnswers(currentQuestion.Answers);
        _selectionUI.UpdateSelectionInUI();
    }

    /// <summary>
    /// Callback invoked when an answer is selected.
    /// </summary>
    /// <param name="selection">Index of the selected answer.</param>
    private void OnAnswerSelected(int selection)
    {
        StartCoroutine(ProcessAnswer(selection));
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

    /// <summary>
    /// Processes the selected answer, updates affinity, and advances the recruitment dialogue.
    /// </summary>
    /// <param name="selectedAnswerIndex">Index of the selected answer.</param>
    private IEnumerator ProcessAnswer(int selectedAnswerIndex)
    {
        // Disable selection UI and unsubscribe to prevent multiple triggers.
        _selectionUI.gameObject.SetActive(false);
        _selectionUI.OnSelected -= OnAnswerSelected;

        RecruitmentQuestion currentQuestion = _selectedQuestions[_currentQuestionIndex];
        RecruitmentAnswer selectedAnswer = currentQuestion.Answers[selectedAnswerIndex];

        int oldAffinity = RecruitTarget.Battler.AffinityLevel;
        RecruitTarget.Battler.UpdateAffinityLevel(selectedAnswer.AffinityScore);
        int newAffinity = RecruitTarget.Battler.AffinityLevel;

        // Play corresponding affinity animations if there is a change.
        if (newAffinity != oldAffinity)
        {
            if (newAffinity > oldAffinity)
            {
                StartCoroutine(RecruitTarget.PlayAffinityGainAnimation());
            }
            else if (newAffinity < oldAffinity)
            {
                StartCoroutine(RecruitTarget.PlayAffinityLossAnimation());
            }
            yield return RecruitTarget.Hud.SetAffinitySmooth();
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
        }

        _dialogueBox.EnableDialogueText(true);
        yield return _dialogueBox.TypeDialogue(GenerateReaction(selectedAnswer.AffinityScore));

        // If more questions remain, present the next one; otherwise, attempt recruitment.
        if (_currentQuestionIndex < _selectedQuestions.Count - 1)
        {
            _currentQuestionIndex++;
            yield return PresentQuestion();
        }
        else
        {
            yield return AttemptRecruitment();
        }
    }

    /// <summary>
    /// Generates a reaction string based on the affinity score.
    /// </summary>
    /// <param name="affinityScore">The affinity score of the selected answer.</param>
    /// <returns>A reaction string.</returns>
    private string GenerateReaction(int affinityScore)
    {
        string name = RecruitTarget.Battler.Base.Name;
        return affinityScore == 2
            ? $"{name} seems to love your answer!"
            : affinityScore == 1
                ? $"{name} seems to like your answer."
                : affinityScore == -1 ? $"{name} seems to dislike your answer..." : $"{name} seems to hate your answer!";
    }

    /// <summary>
    /// Attempts to recruit the target by calculating the recruitment chance.
    /// </summary>
    private IEnumerator AttemptRecruitment()
    {
        bool canRecruit = CanRecruit();

        if (canRecruit)
        {
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Battler.Base.Name} wants to join your party! Will you accept?");

            // Present choice to accept or reject.
            _dialogueBox.EnableDialogueText(false);
            _dialogueBox.EnableChoiceBox(true);
            _yesSelected = true;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }
        else
        {
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Battler.Base.Name} refused to join you.");
            if (RecruitTarget.Battler.AffinityLevel == 0)
            {
                RecruitTarget.Hud.ToggleAffinityBar(false);
            }
            _battleSystem.StateMachine.Pop();
        }
    }

    /// <summary>
    /// Calculates the recruitment chance and returns whether recruitment is successful.
    /// </summary>
    /// <returns>True if recruitment can occur; otherwise, false.</returns>
    private bool CanRecruit()
    {
        float a = Mathf.Min(Mathf.Max(RecruitTarget.Battler.AffinityLevel - 3, 0), 3) *
                  ((3 * RecruitTarget.Battler.MaxHp) - (2 * RecruitTarget.Battler.Hp)) *
                  RecruitTarget.Battler.Base.RecruitRate *
                  ConditionsDB.GetStatusBonus(RecruitTarget.Battler.Statuses) /
                  (3 * RecruitTarget.Battler.MaxHp);
        if (a >= 255)
        {
            return true;
        }
        else
        {
            float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));
            return Random.Range(0, 65536) < b;
        }
    }

    /// <summary>
    /// Processes the player's accept/reject choice for recruitment.
    /// </summary>
    /// <param name="selection">0 for Yes, 1 for No.</param>
    private IEnumerator ProcessAcceptReject(int selection)
    {
        _dialogueBox.EnableDialogueText(true);

        if (selection == 0)
        {
            // Recruitment accepted.
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Battler.Base.Name} was recruited!");
            StartCoroutine(RecruitTarget.PlayExitAnimation());
            RecruitTarget.ClearData();
            _battleSystem.EnemyUnits.Remove(RecruitTarget);
            _battleSystem.PlayerParty.AddMember(RecruitTarget.Battler);
            AudioManager.Instance.PlaySFX(AudioID.BattlerObtained);
            while (AudioManager.Instance.SfxPlayer.isPlaying)
            {
                yield return null;
            }

            if (_battleSystem.EnemyUnits.Count == 0)
            {
                AudioManager.Instance.PlayMusic(_battleSystem.BattleWonMusic, loop: false);
                yield return _dialogueBox.TypeDialogue("There are no more enemies remaining!");
                yield return _dialogueBox.TypeDialogue("You are victorious!", clearDialogue: false);
                while (AudioManager.Instance.MusicPlayer.isPlaying)
                {
                    yield return null;
                }
                _battleSystem.BattleOver(true);
            }
            else
            {
                AdjustBattleActionsAfterRecruitment();
            }
        }
        else
        {
            // Recruitment rejected.
            yield return _dialogueBox.TypeDialogue($"{RecruitTarget.Battler.Base.Name} was rejected.");
            if (RecruitTarget.Battler.AffinityLevel == 0)
            {
                RecruitTarget.Hud.ToggleAffinityBar(false);
            }
        }

        _battleSystem.StateMachine.Pop();
    }

    /// <summary>
    /// Adjusts battle actions after a successful recruitment to remove references to the recruited unit.
    /// </summary>
    private void AdjustBattleActionsAfterRecruitment()
    {
        BattleAction recruitedAction = RunTurnState.Instance.BattleActions.FirstOrDefault(a => a.SourceUnit == RecruitTarget);
        if (recruitedAction != null)
        {
            recruitedAction.IsValid = false;
        }

        List<BattleAction> actionsToAdjust = RunTurnState.Instance.BattleActions
            .Where(a => a.TargetUnits != null && a.TargetUnits.Contains(RecruitTarget))
            .ToList();
        foreach (BattleAction action in actionsToAdjust)
        {
            action.TargetUnits.Remove(RecruitTarget);
            if (action.TargetUnits.Count == 0)
            {
                action.TargetUnits.Add(_battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]);
            }
        }
    }

    /// <summary>
    /// Handles user input when the choice box is active.
    /// </summary>
    private void HandleChoiceBoxInput()
    {
        const float SELECTION_SPEED = 5f;
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Countdown timer to control selection speed.
        if (_selectionTimer > 0)
        {
            _selectionTimer = Mathf.Clamp(_selectionTimer - Time.deltaTime, 0, _selectionTimer);
        }

        if (_selectionTimer == 0 && Mathf.Abs(verticalInput) > 0.2f)
        {
            _yesSelected = !_yesSelected;
            _selectionTimer = 1 / SELECTION_SPEED;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
            AudioManager.Instance.PlaySFX(AudioID.UIShift);
        }

        if (Input.GetButtonDown("Action"))
        {
            _dialogueBox.EnableChoiceBox(false);
            AudioManager.Instance.PlaySFX(AudioID.UISelect);
            int selection = _yesSelected ? 0 : 1;
            StartCoroutine(ProcessAcceptReject(selection));
        }

        if (Input.GetButtonDown("Back"))
        {
            _dialogueBox.EnableChoiceBox(false);
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            StartCoroutine(ProcessAcceptReject(1));
        }
    }
}
