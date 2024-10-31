using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private string description;
    [SerializeField] private Dialogue startDialogue;
    [SerializeField] private Dialogue inProgressDialogue;
    [SerializeField] private Dialogue completedDialogue;
    [SerializeField] private ItemBase requiredItem;
    [SerializeField] private ItemBase rewardItem;

    public string Name => name;
    public string Description => description;
    public Dialogue StartDialogue => startDialogue;
    public Dialogue InProgressDialogue => inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue;
    public Dialogue CompletedDialogue => completedDialogue;
    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
