using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailableWater : MonoBehaviour, Interactable
{
    [SerializeField] ItemBase requiredItem;

    public IEnumerator Interact(Transform initiator)
    {
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

                var animator = initiator.GetComponent<CharacterAnimator>();
                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                animator.IsSailing = true;
            }
        }
    }
}
