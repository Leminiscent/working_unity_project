using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour, ISavable
{
    private List<Quest> _quests = new();

    public event Action OnUpdated;

    public void AddQuest(Quest quest)
    {
        if (!_quests.Contains(quest))
        {
            _quests.Add(quest);
        }

        OnUpdated?.Invoke();
    }

    public bool IsStarted(string questName)
    {
        QuestStatus? questStatus = _quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;

        return questStatus is QuestStatus.Started or QuestStatus.Completed;
    }

    public bool IsCompleted(string questName)
    {
        QuestStatus? questStatus = _quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;

        return questStatus == QuestStatus.Completed;
    }

    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    public object CaptureState()
    {
        return _quests.Select(static q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        if (state is List<QuestSaveData> saveData)
        {
            _quests = saveData.Select(static q => new Quest(q)).ToList();
            OnUpdated?.Invoke();
        }
    }
}
