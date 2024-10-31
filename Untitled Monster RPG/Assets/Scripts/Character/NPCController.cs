using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] private Dialogue dialogue;

    [Header("Quests")]
    [SerializeField] private QuestBase questToStart;
    [SerializeField] private QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] private List<Vector2> movementPattern;
    [SerializeField] private float patternRate;
    private NPCState state;
    private float idleTimer = 0f;
    private int currentPattern = 0;
    private Quest activeQuest;
    private Character character;
    private ItemGiver itemGiver;
    private MonsterGiver monsterGiver;
    private Healer healer;
    private Merchant merchant;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        monsterGiver = GetComponent<MonsterGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Talking;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
            {
                Quest quest = new Quest(questToComplete);

                yield return quest.CompleteQuest(initiator);
                questToComplete = null;
            }

            if (itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (monsterGiver != null && monsterGiver.CanBeGiven())
            {
                yield return monsterGiver.GiveMonster(initiator.GetComponent<PlayerController>());
            }
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }
            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogueManager.Instance.ShowDialogue(activeQuest.Base.InProgressDialogue);
                }
            }
            else if (healer != null)
            {
                yield return healer.Heal(initiator, dialogue);
            }
            else if (merchant != null)
            {
                yield return merchant.Trade();
            }
            else
            {
                yield return DialogueManager.Instance.ShowDialogue(dialogue);
            }

            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > patternRate)
            {
                idleTimer = 0f;
                if (movementPattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }

        character.HandleUpdate();
    }

    private IEnumerator Walk()
    {
        state = NPCState.Moving;

        Vector3 prevPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);
        if (transform.position != prevPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }
        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        NPCSaveData saveData = new NPCSaveData
        {
            position = new float[] { transform.position.x, transform.position.y },
            facingDirection = character.Animator.FacingDirection,
            activeQuest = activeQuest?.GetSaveData()
        };

        if (questToStart != null)
        {
            saveData.questToStart = new Quest(questToStart).GetSaveData();
        }

        if (questToComplete != null)
        {
            saveData.questToComplete = new Quest(questToComplete).GetSaveData();
        }

        return saveData;
    }

    public void RestoreState(object state)
    {
        NPCSaveData saveData = (NPCSaveData)state;

        if (saveData != null)
        {
            transform.position = new Vector3(saveData.position[0], saveData.position[1]);
            character.Animator.FacingDirection = saveData.facingDirection;
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCSaveData
{
    public float[] position;
    public FacingDirection facingDirection;
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Moving, Talking }