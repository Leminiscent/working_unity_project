using System.Collections;
using UnityEngine;

public class BattlerGiver : MonoBehaviour, ISavable
{
    [SerializeField] private Battler _battler;
    [SerializeField] private Dialogue _dialogue;

    private bool _used = false;

    public IEnumerator GiveBattler(PlayerController player)
    {
        yield return DialogueManager.Instance.ShowDialogue(_dialogue);
        _battler.Init();
        player.GetComponent<BattleParty>().AddMember(_battler);
        _used = true;
        AudioManager.Instance.PlaySFX(AudioID.BattlerObtained, pauseMusic: true);
        yield return DialogueManager.Instance.ShowDialogueText($"{player.Name} received {_battler.Base.Name}!");
    }

    public bool CanBeGiven()
    {
        return _battler != null && !_used;
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
