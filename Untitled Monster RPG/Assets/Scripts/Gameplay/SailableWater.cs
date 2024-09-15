using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailableWater : MonoBehaviour, Interactable, IPlayerTriggerable
{
    [SerializeField] ItemBase requiredItem;
    bool isJumpingToWater = false;

    public bool TriggerRepeatedly => true;

    public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();

        if (animator.IsSailing || isJumpingToWater)
        {
            yield break;
        }

        yield return DialogueManager.Instance.ShowDialogueText("This water is too deep to cross on foot.");
        if (initiator.GetComponent<Inventory>().HasItem(requiredItem))
        {
            int selectedChoice = 0;
            string playerName = initiator.GetComponent<PlayerController>().Name;

            yield return DialogueManager.Instance.ShowDialogueText("Would you like to set sail?",
                choices: new List<string> { "Yes", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                yield return DialogueManager.Instance.ShowDialogueText($"{playerName} is setting sail!");

                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                isJumpingToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpingToWater = false;
                animator.IsSailing = true;
            }
        }
    }

    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 5)
        {
            GameController.Instance.StartWildBattle(BattleTrigger.Water);
        }
    }
}
