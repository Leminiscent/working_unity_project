using System.Collections;
using UnityEngine;

public class BattlerGiver : MonoBehaviour, ISavable
{
    [SerializeField] private Battler _battler;
    [SerializeField] private Dialogue _dialogue;

    private bool _used = false;

    public IEnumerator GiveBattler(PlayerController player)
    {
        // Show initial dialogue.
        yield return DialogueManager.Instance.ShowDialogue(_dialogue);

        // Initialize the battler with the new initialization method.
        _battler.InitBattler();

        // Add the battler to the player's party.
        player.GetComponent<BattleParty>().AddMember(_battler);

        // Mark as used and play the battler-obtained sound effect.
        _used = true;
        AudioManager.Instance.PlaySFX(AudioID.BattlerObtained, pauseMusic: true);

        // Display the notification dialogue.
        yield return DialogueManager.Instance.ShowDialogueText($"{_battler.Base.Name} was recruited!");
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