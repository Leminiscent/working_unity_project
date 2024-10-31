using System.Collections;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public QuestSaveData GetSaveData()
    {
        QuestSaveData saveData = new QuestSaveData
        {
            name = Base.name,
            status = Status
        };

        return saveData;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
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

        if (Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
            {
                return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None, Started, Completed }