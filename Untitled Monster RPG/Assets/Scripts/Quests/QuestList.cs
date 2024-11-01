using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour, ISavable
{
    private List<Quest> quests = new();

    public event Action OnUpdated;

    public void AddQuest(Quest quest)
    {
        if (!quests.Contains(quest))
        {
            quests.Add(quest);
        }

        OnUpdated?.Invoke();
    }

    public bool IsStarted(string questName)
    {
        QuestStatus? questStatus =  quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;

        return questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }

    public bool IsCompleted(string questName)
    {
        QuestStatus? questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;

        return questStatus == QuestStatus.Completed;
    }

    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    public object CaptureState()
    {
        return quests.Select(static q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        List<QuestSaveData> saveData = state as List<QuestSaveData>;

        if (saveData != null)
        {
            quests = saveData.Select(static q => new Quest(q)).ToList();
            OnUpdated?.Invoke();
        }
    }
}
