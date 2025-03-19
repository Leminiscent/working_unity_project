using System.Collections;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase questBase)
    {
        Base = questBase;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.Name);
        Status = saveData.Status;
    }

    public QuestSaveData GetSaveData()
    {
        QuestSaveData saveData = new()
        {
            Name = Base.name,
            Status = Status
        };

        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;
        yield return DialogueManager.Instance.ShowDialogue(Base.StartDialogue);

        QuestList questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;
        yield return DialogueManager.Instance.ShowDialogue(Base.CompletedDialogue);

        Inventory inventory = Inventory.GetInventory();

        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }
        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);
            string playerName = player.GetComponent<PlayerController>().Name;
            yield return DialogueManager.Instance.ShowDialogueText($"{playerName} received {Base.RewardItem.Name}!");
        }

        QuestList questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        Inventory inventory = Inventory.GetInventory();
        return Base.RequiredItem == null || inventory.HasItem(Base.RequiredItem);
    }
}

[System.Serializable]
public class QuestSaveData
{
    public string Name;
    public QuestStatus Status;
}

public enum QuestStatus
{
    None,
    Started,
    Completed
}