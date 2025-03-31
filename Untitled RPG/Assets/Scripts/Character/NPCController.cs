using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] private Dialogue _dialogue;

    [Header("Quests")]
    [SerializeField] private QuestBase _questToStart;
    [SerializeField] private QuestBase _questToComplete;

    [Header("Movement")]
    [SerializeField] private List<Vector2> _movementPattern;
    [SerializeField] private float _patternRate;

    private NPCState _state;
    private float _idleTimer = 0f;
    private int _currentPattern = 0;
    private Quest _activeQuest;
    private Character _character;
    private ItemGiver _itemGiver;
    private BattlerGiver _battlerGiver;
    private Healer _healer;
    private Merchant _merchant;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _itemGiver = GetComponent<ItemGiver>();
        _battlerGiver = GetComponent<BattlerGiver>();
        _healer = GetComponent<Healer>();
        _merchant = GetComponent<Merchant>();
    }

    private void Update()
    {
        // If idle, update the idle timer and start movement pattern if necessary.
        if (_state == NPCState.Idle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > _patternRate && _movementPattern.Count > 0)
            {
                _idleTimer = 0f;
                _ = StartCoroutine(Walk());
            }
        }

        _character.UpdateAnimator();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (_state != NPCState.Idle)
        {
            yield break;
        }

        _state = NPCState.Talking;
        _character.LookTowards(initiator.position);

        // Handle quest completion if available.
        if (_questToComplete != null)
        {
            Quest questToComplete = new(_questToComplete);
            yield return questToComplete.CompleteQuest(initiator);
            _questToComplete = null;
        }

        // Give item if available.
        if (_itemGiver != null && _itemGiver.CanBeGiven())
        {
            yield return _itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
        }
        // Give battler if available.
        else if (_battlerGiver != null && _battlerGiver.CanBeGiven())
        {
            yield return _battlerGiver.GiveBattler(initiator.GetComponent<PlayerController>());
        }
        // Start a new quest if available.
        else if (_questToStart != null)
        {
            _activeQuest = new Quest(_questToStart);
            yield return _activeQuest.StartQuest();
            _questToStart = null;

            if (_activeQuest.CanBeCompleted())
            {
                yield return _activeQuest.CompleteQuest(initiator);
                _activeQuest = null;
            }
        }
        // Handle active quest progression.
        else if (_activeQuest != null)
        {
            if (_activeQuest.CanBeCompleted())
            {
                yield return _activeQuest.CompleteQuest(initiator);
                _activeQuest = null;
            }
            else
            {
                if (_activeQuest.Base.InProgressDialogue != null && _activeQuest.Base.InProgressDialogue.Lines.Count > 0)
                {
                    yield return DialogueManager.Instance.ShowDialogue(_activeQuest.Base.InProgressDialogue);
                }
            }
        }
        // Fallback dialogue or service interaction (Healer or Merchant).
        else
        {
            yield return _healer != null
                ? _healer.Heal(initiator)
                : _merchant != null ? _merchant.Trade() : DialogueManager.Instance.ShowDialogue(_dialogue);
        }

        _idleTimer = 0f;
        _state = NPCState.Idle;
    }

    public object CaptureState()
    {
        NPCSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _character.Animator.FacingDirection,
            ActiveQuest = _activeQuest?.GetSaveData(),
            QuestToStart = _questToStart != null ? new Quest(_questToStart).GetSaveData() : null,
            QuestToComplete = _questToComplete != null ? new Quest(_questToComplete).GetSaveData() : null
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        NPCSaveData saveData = (NPCSaveData)state;

        if (saveData != null)
        {
            transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
            _character.Animator.FacingDirection = saveData.FacingDirection;
            _activeQuest = saveData.ActiveQuest != null ? new Quest(saveData.ActiveQuest) : null;
            _questToStart = saveData.QuestToStart != null ? new Quest(saveData.QuestToStart).Base : null;
            _questToComplete = saveData.QuestToComplete != null ? new Quest(saveData.QuestToComplete).Base : null;
        }
    }

    private IEnumerator Walk()
    {
        _state = NPCState.Moving;
        Vector3 previousPosition = transform.position;

        yield return _character.MoveRoutine(_movementPattern[_currentPattern]);

        if (transform.position != previousPosition)
        {
            _currentPattern = (_currentPattern + 1) % _movementPattern.Count;
        }

        _state = NPCState.Idle;
    }
}

public enum NPCState
{
    Idle,
    Moving,
    Talking
}

[System.Serializable]
public class NPCSaveData
{
    public float[] Position;
    public FacingDirection FacingDirection;
    public QuestSaveData ActiveQuest;
    public QuestSaveData QuestToStart;
    public QuestSaveData QuestToComplete;
}