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
        if (Base.StartDialogue != null && Base.StartDialogue.Lines.Count > 0)
        {
            yield return DialogueManager.Instance.ShowDialogue(Base.StartDialogue);
        }

        QuestList questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;
        if (Base.CompleteDialogue != null && Base.CompleteDialogue.Lines.Count > 0)
        {
            yield return DialogueManager.Instance.ShowDialogue(Base.CompleteDialogue);
        }

        Inventory inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        ItemBase reward = Base.RewardItem;
        if (reward != null)
        {
            inventory.AddItem(reward);
            yield return DialogueManager.Instance.ShowDialogueText($"{player.GetComponent<PlayerController>().Name} has received {TextUtil.GetArticle(reward.Name)} {reward.Name}!");
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