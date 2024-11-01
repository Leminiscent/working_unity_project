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
    private MonsterGiver _monsterGiver;
    private Healer _healer;
    private Merchant _merchant;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _itemGiver = GetComponent<ItemGiver>();
        _monsterGiver = GetComponent<MonsterGiver>();
        _healer = GetComponent<Healer>();
        _merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (_state == NPCState.Idle)
        {
            _state = NPCState.Talking;
            _character.LookTowards(initiator.position);

            if (_questToComplete != null)
            {
                Quest quest = new(_questToComplete);

                yield return quest.CompleteQuest(initiator);
                _questToComplete = null;
            }

            if (_itemGiver != null && _itemGiver.CanBeGiven())
            {
                yield return _itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (_monsterGiver != null && _monsterGiver.CanBeGiven())
            {
                yield return _monsterGiver.GiveMonster(initiator.GetComponent<PlayerController>());
            }
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
            else if (_activeQuest != null)
            {
                if (_activeQuest.CanBeCompleted())
                {
                    yield return _activeQuest.CompleteQuest(initiator);
                    _activeQuest = null;
                }
                else
                {
                    yield return DialogueManager.Instance.ShowDialogue(_activeQuest.Base.InProgressDialogue);
                }
            }
            else if (_healer != null)
            {
                yield return _healer.Heal(initiator, _dialogue);
            }
            else if (_merchant != null)
            {
                yield return _merchant.Trade();
            }
            else
            {
                yield return DialogueManager.Instance.ShowDialogue(_dialogue);
            }

            _idleTimer = 0f;
            _state = NPCState.Idle;
        }
    }

    private void Update()
    {
        if (_state == NPCState.Idle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > _patternRate)
            {
                _idleTimer = 0f;
                if (_movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }

        _character.HandleUpdate();
    }

    private IEnumerator Walk()
    {
        _state = NPCState.Moving;

        Vector3 prevPos = transform.position;

        yield return _character.Move(_movementPattern[_currentPattern]);
        if (transform.position != prevPos)
        {
            _currentPattern = (_currentPattern + 1) % _movementPattern.Count;
        }
        _state = NPCState.Idle;
    }

    public object CaptureState()
    {
        NPCSaveData saveData = new()
        {
            Position = new float[] { transform.position.x, transform.position.y },
            FacingDirection = _character.Animator.FacingDirection,
            ActiveQuest = _activeQuest?.GetSaveData()
        };

        if (_questToStart != null)
        {
            saveData.QuestToStart = new Quest(_questToStart).GetSaveData();
        }

        if (_questToComplete != null)
        {
            saveData.QuestToComplete = new Quest(_questToComplete).GetSaveData();
        }

        return saveData;
    }

    public void RestoreState(object state)
    {
        NPCSaveData saveData = (NPCSaveData)state;

        if (saveData != null)
        {
            transform.position = new Vector3(saveData.Position[0], saveData.Position[1]);
            _character.Animator.FacingDirection = saveData.FacingDirection;
            _activeQuest = (saveData.ActiveQuest != null) ? new Quest(saveData.ActiveQuest) : null;
            _questToStart = (saveData.QuestToStart != null) ? new Quest(saveData.QuestToStart).Base : null;
            _questToComplete = (saveData.QuestToComplete != null) ? new Quest(saveData.QuestToComplete).Base : null;
        }
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