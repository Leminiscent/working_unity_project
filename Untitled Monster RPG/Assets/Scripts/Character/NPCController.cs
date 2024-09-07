using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialogue dialogue;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float patternRate;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;
    Character character;
    ItemGiver itemGiver;
    MonsterGiver monsterGiver;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        monsterGiver = GetComponent<MonsterGiver>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Talking;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);

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

    IEnumerator Walk()
    {
        state = NPCState.Moving;

        var prevPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);
        if (transform.position != prevPos)
        {
            currentPattern = (currentPattern + 1) % movementPattern.Count;
        }
        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();

        saveData.activeQuest = activeQuest?.GetSaveData();
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
        var saveData = state as NPCQuestSaveData;

        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;
            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Moving, Talking }