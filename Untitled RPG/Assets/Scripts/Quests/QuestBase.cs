using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [field: SerializeField, FormerlySerializedAs("_name")] public string Name { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_description")] public string Description { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_startDialogue")] public Dialogue StartDialogue { get; private set; }
    [SerializeField] private Dialogue _inProgressDialogue;
    [field: SerializeField, FormerlySerializedAs("_completedDialogue")] public Dialogue CompleteDialogue { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_requiredItem")] public ItemBase RequiredItem { get; private set; }
    [field: SerializeField, FormerlySerializedAs("_rewardItem")] public ItemBase RewardItem { get; private set; }

    public Dialogue InProgressDialogue => _inProgressDialogue?.Lines?.Count > 0 ? _inProgressDialogue : StartDialogue;
}