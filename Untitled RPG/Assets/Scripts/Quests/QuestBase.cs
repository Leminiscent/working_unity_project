using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private string _description;
    [SerializeField] private Dialogue _startDialogue;
    [SerializeField] private Dialogue _inProgressDialogue;
    [SerializeField] private Dialogue _completedDialogue;
    [SerializeField] private ItemBase _requiredItem;
    [SerializeField] private ItemBase _rewardItem;

    public string Name => _name;
    public string Description => _description;
    public Dialogue StartDialogue => _startDialogue;
    public Dialogue InProgressDialogue => _inProgressDialogue?.Lines?.Count > 0 ? _inProgressDialogue : _startDialogue;
    public Dialogue CompletedDialogue => _completedDialogue;
    public ItemBase RequiredItem => _requiredItem;
    public ItemBase RewardItem => _rewardItem;
}
