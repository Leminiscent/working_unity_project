using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    private const float FadeDuration = 0.5f;

    public IEnumerator Heal(Transform player)
    {
        int choiceIndex = 0;

        // Ask the player if they would like to rest.
        yield return DialogueManager.Instance.ShowDialogueText(
            "You look tired! Would you like to rest here?",
            choices: new List<string> { "Yes", "No" },
            onChoiceSelected: (selected) => choiceIndex = selected);

        // If player selects "Yes"
        if (choiceIndex == 0)
        {
            yield return Fader.Instance.FadeIn(FadeDuration);

            BattleParty playerParty = player.GetComponent<BattleParty>();

            playerParty.RestoreParty();
            playerParty.PartyUpdated();

            yield return Fader.Instance.FadeOut(FadeDuration);

            yield return DialogueManager.Instance.ShowDialogueText(
                "All healthy! Come back if you need more healing!");
        }
        // If player selects "No"
        else if (choiceIndex == 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText(
                "Come back if you need healing!");
        }
    }
}