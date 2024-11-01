using System.Collections;
using UnityEngine;

public class MonsterGiver : MonoBehaviour, ISavable
{
    [SerializeField] private Monster _monster;
    [SerializeField] private Dialogue _dialogue;

    private bool _used = false;

    public IEnumerator GiveMonster(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(_dialogue);
        _monster.Init();
        player.GetComponent<MonsterParty>().AddMonster(_monster);
        _used = true;
        AudioManager.Instance.PlaySFX(AudioID.MonsterObtained, pauseMusic: true);
        yield return DialogueManager.Instance.ShowDialogueText($"{player.Name} received {_monster.Base.Name}!");
    }

    public bool CanBeGiven()
    {
        return _monster != null && !_used;
    }

    public object CaptureState()
    {
        return _used;
    }

    public void RestoreState(object state)
    {
        _used = (bool)state;
    }
}
