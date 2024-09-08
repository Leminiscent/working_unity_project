using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialogue dialogue)
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue, new List<string> { "Yes", "No" });
        yield return Fader.Instance.FadeIn(0.5f);

        var playerParty = player.GetComponent<MonsterParty>();

        playerParty.Monsters.ForEach(m => m.Heal());
        playerParty.PartyUpdated();
        yield return Fader.Instance.FadeOut(0.5f);
    }
}
