using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.StateMachine;

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

        _ = StartCoroutine(StartRecruitment());
    }

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

    public override void Exit()
    {
        if (_selectionUI != null)
        {
            _selectionUI.gameObject.SetActive(false);
            _selectionUI.OnSelected -= OnAnswerSelected;
        }
    }

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

    private void OnAnswerSelected(int selection)
    {
        _ = StartCoroutine(ProcessAnswer(selection));
        AudioManager.Instance.PlaySFX(AudioID.UISelect);
    }

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
                _ = StartCoroutine(RecruitTarget.PlayAffinityGainAnimation());
            }
            else if (newAffinity < oldAffinity)
            {
                _ = StartCoroutine(RecruitTarget.PlayAffinityLossAnimation());
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

    private string GenerateReaction(int affinityScore)
    {
        string name = RecruitTarget.Battler.Base.Name;
        return affinityScore == 2
            ? $"The rogue {name} seems to love your answer!"
            : affinityScore == 1
                ? $"The rogue {name} seems to like your answer."
                : affinityScore == -1 ? $"The rogue {name} seems to dislike your answer..." : $"The rogue {name} seems to hate your answer!";
    }

    private IEnumerator AttemptRecruitment()
    {
        bool canRecruit = CanRecruit();

        if (canRecruit)
        {
            yield return _dialogueBox.TypeDialogue($"The rogue {RecruitTarget.Battler.Base.Name} wants to join your cause! Will you accept?");

            // Present choice to accept or reject.
            _dialogueBox.EnableDialogueText(false);
            _dialogueBox.EnableChoiceBox(true);
            _yesSelected = true;
            _dialogueBox.UpdateChoiceBox(_yesSelected);
        }
        else
        {
            yield return _dialogueBox.TypeDialogue($"The rogue {RecruitTarget.Battler.Base.Name} refused to join you.");
            if (RecruitTarget.Battler.AffinityLevel == 0)
            {
                RecruitTarget.Hud.ToggleAffinityBar(false);
            }
            _battleSystem.StateMachine.Pop();
        }
    }

    private bool CanRecruit()
    {
        float a = Mathf.Min(Mathf.Max(RecruitTarget.Battler.AffinityLevel - 3, 0), 3) *
                  ((3 * RecruitTarget.Battler.MaxHp) - (2 * RecruitTarget.Battler.Hp)) *
                  RecruitTarget.Battler.Base.RecruitRate *
                  StatusConditionDB.GetStatusBonus(RecruitTarget.Battler.Statuses) /
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

    private IEnumerator ProcessAcceptReject(int selection)
    {
        _dialogueBox.EnableDialogueText(true);

        if (selection == 0)
        {
            // Recruitment accepted.
            yield return _dialogueBox.TypeDialogue($"The rogue {RecruitTarget.Battler.Base.Name} was recruited!");
            _ = StartCoroutine(RecruitTarget.PlayExitAnimation());
            RecruitTarget.ClearData();
            _ = _battleSystem.EnemyUnits.Remove(RecruitTarget);
            _battleSystem.PlayerParty.AddMember(RecruitTarget.Battler);
            AudioManager.Instance.PlaySFX(AudioID.BattlerObtained);
            while (AudioManager.Instance.SfxPlayer.isPlaying)
            {
                yield return null;
            }

            if (_battleSystem.EnemyUnits.Count == 0)
            {
                AudioManager.Instance.PlayMusic(_battleSystem.BattleWonMusic, loop: false);
                yield return _dialogueBox.TypeDialogue("There are no enemies remaining!");
                yield return _dialogueBox.TypeDialogue("You are victorious!", clearDialogue: false);
                while (AudioManager.Instance.MusicPlayer.isPlaying)
                {
                    yield return null;
                }
                _ = StartCoroutine(_battleSystem.BattleOver(true));
            }
            else
            {
                AdjustBattleActionsAfterRecruitment();
            }
        }
        else
        {
            // Recruitment rejected.
            yield return _dialogueBox.TypeDialogue($"The rogue {RecruitTarget.Battler.Base.Name} was rejected.");
            if (RecruitTarget.Battler.AffinityLevel == 0)
            {
                RecruitTarget.Hud.ToggleAffinityBar(false);
            }
        }

        _battleSystem.StateMachine.Pop();
    }

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
            _ = action.TargetUnits.Remove(RecruitTarget);
            if (action.TargetUnits.Count == 0)
            {
                action.TargetUnits.Add(_battleSystem.EnemyUnits[Random.Range(0, _battleSystem.EnemyUnits.Count)]);
            }
        }
    }

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
            _ = StartCoroutine(ProcessAcceptReject(selection));
        }

        if (Input.GetButtonDown("Back"))
        {
            _dialogueBox.EnableChoiceBox(false);
            AudioManager.Instance.PlaySFX(AudioID.UIReturn);
            _ = StartCoroutine(ProcessAcceptReject(1));
        }
    }
}
