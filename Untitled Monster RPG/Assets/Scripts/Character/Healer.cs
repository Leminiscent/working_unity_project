using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialogue dialogue)
    {
        int selectedChoice = 0;

        yield return DialogueManager.Instance.ShowDialogueText("You look tired! Would you like to rest here?", choices: new List<string> { "Yes", "No" }, onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);
        if (selectedChoice == 0)
        {
            yield return Fader.Instance.FadeIn(0.5f);

            var playerParty = player.GetComponent<MonsterParty>();

            playerParty.Monsters.ForEach(m => m.Heal());
            playerParty.PartyUpdated();
            yield return Fader.Instance.FadeOut(0.5f);
            yield return DialogueManager.Instance.ShowDialogueText("All healed up! Come back if you need more healing!", true);
        }
        else if (selectedChoice == 1)
        {
            yield return DialogueManager.Instance.ShowDialogueText("Come back if you need healing!", true);
        }
    }
}
