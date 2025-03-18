using System.Collections;
using UnityEngine;

/// <summary>
/// Provides a battler to the player during gameplay by triggering dialogue and adding the battler to the player's party.
/// </summary>
public class BattlerGiver : MonoBehaviour, ISavable
{
    [SerializeField] private Battler _battler;
    [SerializeField] private Dialogue _dialogue;

    private bool _used = false;

    /// <summary>
    /// Gives the battler to the player. Shows dialogue, initializes the battler,
    /// adds it to the player's party, and plays the appropriate sound effect.
    /// </summary>
    /// <param name="player">The player controller receiving the battler.</param>
    /// <returns>An IEnumerator for coroutine execution.</returns>
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
        yield return DialogueManager.Instance.ShowDialogueText($"{player.Name} received {_battler.Base.Name}!");
    }

    /// <summary>
    /// Determines whether the battler can be given.
    /// </summary>
    /// <returns>True if a battler is available and it hasn't been given already; otherwise, false.</returns>
    public bool CanBeGiven()
    {
        return _battler != null && !_used;
    }

    /// <summary>
    /// Captures the current state of the BattlerGiver.
    /// </summary>
    /// <returns>An object representing the current state (in this case, the _used flag).</returns>
    public object CaptureState()
    {
        return _used;
    }

    /// <summary>
    /// Restores the state of the BattlerGiver.
    /// </summary>
    /// <param name="state">The state to restore from.</param>
    public void RestoreState(object state)
    {
        _used = (bool)state;
    }
}