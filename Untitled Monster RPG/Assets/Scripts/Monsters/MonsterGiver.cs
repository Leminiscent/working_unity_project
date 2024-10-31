using System.Collections;
using UnityEngine;

public class MonsterGiver : MonoBehaviour, ISavable
{
    [SerializeField] private Monster monster;
    [SerializeField] private Dialogue dialogue;
    private bool used = false;

    public IEnumerator GiveMonster(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(dialogue);
        monster.Init();
        player.GetComponent<MonsterParty>().AddMonster(monster);
        used = true;
        AudioManager.Instance.PlaySFX(AudioID.MonsterObtained, pauseMusic: true);
        yield return DialogueManager.Instance.ShowDialogueText($"{player.Name} received {monster.Base.Name}!");
    }

    public bool CanBeGiven()
    {
        return monster != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
