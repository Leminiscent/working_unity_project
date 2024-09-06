using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] string description;
    [SerializeField] Dialogue startDialogue;
    [SerializeField] Dialogue inProgressDialogue;
    [SerializeField] Dialogue completedDialogue;
    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    public string Name => name;
    public string Description => description;
    public Dialogue StartDialogue => startDialogue;
    public Dialogue InProgressDialogue => inProgressDialogue?.Lines?.Count > 0 ? inProgressDialogue : startDialogue;
    public Dialogue CompletedDialogue => completedDialogue;
    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
